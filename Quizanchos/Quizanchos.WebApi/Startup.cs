using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Threading.RateLimiting;
using Quizanchos.Core;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Implementations;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Quiz.Util;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Extensions;
using Quizanchos.WebApi.Services;
using Quizanchos.WebApi.Services.Auth;
using Quizanchos.WebApi.Services.GameLogic;
using Quizanchos.WebApi.Services.Market;
using Quizanchos.WebApi.Services.PluginSystem;
using Quizanchos.WebApi.Services.Rooms;
using Quizanchos.WebApi.Services.Users;
using Quizanchos.WebApi.Util;
using Quizanchos.WebApi.Hubs;
using Quizanchos.WebApi.Options;
using Quizanchos.WebApi.Services.Payment;
using Quizanchos.WebApi.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Serilog.Extensions.Logging;
using System.Runtime.Loader;
using System.Reflection;

namespace Quizanchos.WebApi;

public static class Startup
{
    public static void Configure(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddlewareExtension>();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        MountPluginStaticFiles(app);

        app.UseRouting();

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hubs/game");
        app.MapHub<GameRoomHub>("/hubs/room");
        app.MapHealthChecks("/health");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }

    public static void AddAuthorizaiton(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = PasswordPolicy.RequireDigit;
            options.Password.RequireLowercase = PasswordPolicy.RequireLowercase;
            options.Password.RequireUppercase = PasswordPolicy.RequireUppercase;
            options.Password.RequireNonAlphanumeric = PasswordPolicy.RequireNonAlphanumeric;
            options.Password.RequiredLength = PasswordPolicy.MinLength;
            options.Password.RequiredUniqueChars = PasswordPolicy.RequiredUniqueChars;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<QuizanchosDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication()
        .AddCookie()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = configuration.GetOption("Auth:Google:ClientId") 
                ?? throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientId");
            googleOptions.ClientSecret = configuration.GetOption("Auth:Google:ClientSecret") 
                ?? throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Google:ClientSecret");
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/Signin");
            options.AccessDeniedPath = "/Signin";
            options.Cookie = new CookieBuilder
            {
                Name = "QAuth",
            };
            if (!int.TryParse(configuration.GetOption("Auth:Cookie:TokenValidityInMinutes"), out int tokenValidityInMinutes))
            {
                throw Util.CriticalExceptionFactory.CreateConfigException("Auth:Cookie:TokenValidityInMinutes");
            }
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenValidityInMinutes);
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppRole.Owner, policy => policy.RequireRole(AppRole.Owner));
            options.AddPolicy(AppRole.Admin, policy => policy.RequireRole(AppRole.Owner, AppRole.Admin));
            options.AddPolicy(AppRole.User, policy => policy.RequireRole(AppRole.Owner, AppRole.Admin, AppRole.User));
        });
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw Util.CriticalExceptionFactory.CreateConfigException("DefaultConnection");

        // Single unified DbContext for all entities
        services.AddDbContext<QuizanchosDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistoryQuizanshos", "entity_framework");
            });
        }, ServiceLifetime.Scoped);

        services.AddHealthChecks().AddDbContextCheck<QuizanchosDbContext>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy("contact", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));
        });

        // ========== MINIGAME REGISTRATION SYSTEM ==========
        // First-party minigames are loaded from assemblies in the host's output directory
        // (built via the MinigamePluginProject MSBuild target in the .csproj).
        var pluginAssemblies = LoadPluginAssemblies();
        var minigameDescriptors = CreateDescriptors<IMinigameDescriptor>(pluginAssemblies).ToList();
        var frontendDescriptors = CreateDescriptors<IMinigameFrontendDescriptor>(pluginAssemblies).ToList();

        // Third-party minigames are loaded from a configurable plugin root, each in its
        // own folder with an isolated AssemblyLoadContext (see PluginLoadContext).
        var pluginRoot = configuration.GetSection("Plugins")["Root"];
        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            pluginRoot = Path.Combine(builder.Environment.ContentRootPath, "plugins");
        }
        else if (!Path.IsPathRooted(pluginRoot))
        {
            pluginRoot = Path.Combine(builder.Environment.ContentRootPath, pluginRoot);
        }

        using var pluginLoaderFactory = new SerilogLoggerFactory(Serilog.Log.Logger, dispose: false);
        var pluginLoaderLogger = pluginLoaderFactory.CreateLogger("PluginLoader");
        var thirdPartyPlugins = PluginLoader.LoadFromDirectory(pluginRoot, pluginLoaderLogger);

        foreach (var plugin in thirdPartyPlugins)
        {
            minigameDescriptors.AddRange(plugin.Descriptors);
            frontendDescriptors.AddRange(plugin.FrontendDescriptors);
        }

        services.AddSingleton<IPluginCatalog>(new PluginCatalog(thirdPartyPlugins));

        var registry = BuildMinigameRegistry(services, minigameDescriptors);
        services.AddSingleton<IMinigameRegistry>(registry);

        // ========== FRONTEND MINIGAME REGISTRATION SYSTEM ==========
        var frontendRegistry = BuildFrontendRegistry(frontendDescriptors);
        services.AddSingleton<IMinigameFrontendRegistry>(frontendRegistry);
        // ===================================================

        services.AddAutoMapper(cfg => 
        { 
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<QuizMappingProfile>(); 
        });

        services.AddSingleton<ICloudinary>(serviceProvider =>
        {
            Account account = new Account(
                configuration.GetOption("Cloudinary:CloudName"),
                configuration.GetOption("Cloudinary:ApiKey"),
                configuration.GetOption("Cloudinary:ApiSecret")
            );
            return new Cloudinary(account);
        });

        services.AddSingleton<ContainerService>();

        AddControllers(builder, minigameDescriptors);

        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IGameSessionStateRepository, GameSessionStateRepository>();
        services.AddScoped<IGameStatePersistence, GameStatePersistence>();
        services.AddScoped<IMarketItemRepository, MarketItemRepository>();
        services.AddScoped<IUserOwnedItemRepository, UserOwnedItemRepository>();
        services.AddScoped<IGameLogicFactory, GameLogicFactory>();
        services.AddScoped<IUserMinigameScoreRepository, UserMinigameScoreRepository>();
        services.AddScoped<PremiumAccessService>();
        services.AddScoped<UserScoreService>();
        services.AddScoped<MarketService>();
        services.AddScoped<GameService>();
        services.AddSignalR();
        services.AddScoped<Quizanchos.Core.IGameNotifier, SignalRGameNotifier>();
        services.AddSingleton<IGameRoomManager, InMemoryGameRoomManager>();
        services.AddScoped<Quizanchos.Core.IRoomNotifier, SignalRRoomNotifier>();
        services.AddScoped<GameRoomService>();
        services.AddScoped<AdminService>();
        services.AddScoped<StatisticsService>();
        services.AddTransient<UserRetrieverService>();
        services.AddTransient<GoogleAuthorizationService>();
        services.AddTransient<AuthorizationService>(); 
        services.AddScoped<UserProfileService>();
        services.AddTransient<LeaderBoardService>();

        services.Configure<ContactOptions>(configuration.GetSection("Contact"));
        services.AddScoped<ContactService>();

        services.Configure<BinanceApiOptions>(configuration.GetSection("BinanceApi"));
        services.Configure<List<CoinPackageOption>>(configuration.GetSection("CoinPackages"));
        services.AddHttpClient("Binance", c => c.BaseAddress = new Uri("https://api.binance.com"));
        services.AddScoped<BinanceDepositService>();
        services.AddScoped<TopUpService>();
        services.AddScoped<ITopUpOrderRepository, TopUpOrderRepository>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("MonthlyTask");
            q.AddJob<LeadersUpdaterJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MonthlyTaskTrigger")
                .WithCronSchedule("0 0 0 1 * ?"));

            var depositCheckerKey = new JobKey("DepositChecker");
            q.AddJob<DepositCheckerJob>(opts => opts.WithIdentity(depositCheckerKey));
            q.AddTrigger(opts => opts
                .ForJob(depositCheckerKey)
                .WithIdentity("DepositCheckerTrigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));

            var orderExpiryKey = new JobKey("OrderExpiry");
            q.AddJob<OrderExpiryJob>(opts => opts.WithIdentity(orderExpiryKey));
            q.AddTrigger(opts => opts
                .ForJob(orderExpiryKey)
                .WithIdentity("OrderExpiryTrigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public async static Task SeedData(this WebApplication app, ConfigurationManager configuration)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            QuizanchosDbContext dbContext = services.GetRequiredService<QuizanchosDbContext>();
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);
            await DataSeeder.SeedDatabase(services, configuration);
        }
    }

    private static IReadOnlyList<Assembly> LoadPluginAssemblies()
    {
        var loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            if (!loadedAssemblies.ContainsKey(assembly.FullName ?? assembly.GetName().Name ?? string.Empty))
            {
                loadedAssemblies[assembly.FullName ?? assembly.GetName().Name ?? string.Empty] = assembly;
            }
        }

        var rootAssembly = Assembly.GetExecutingAssembly();
        var dependencyNames = rootAssembly
            .GetReferencedAssemblies()
            .Where(x => x.Name?.StartsWith("Quizanchos.", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        foreach (var dependencyName in dependencyNames)
        {
            try
            {
                var assembly = Assembly.Load(dependencyName);
                var key = assembly.FullName ?? assembly.GetName().Name ?? string.Empty;
                if (!loadedAssemblies.ContainsKey(key))
                {
                    loadedAssemblies[key] = assembly;
                }
            }
            catch
            {
                // Ignore optional/unloadable assemblies
            }
        }

        var baseDirectory = AppContext.BaseDirectory;
        if (Directory.Exists(baseDirectory))
        {
            foreach (var dllPath in Directory.EnumerateFiles(baseDirectory, "Quizanchos.*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dllPath);
                    var loaded = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

                    var assembly = loaded ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                    var key = assembly.FullName ?? assembly.GetName().Name ?? string.Empty;

                    if (!loadedAssemblies.ContainsKey(key))
                    {
                        loadedAssemblies[key] = assembly;
                    }
                }
                catch
                {
                    // Ignore optional/unloadable assemblies
                }
            }
        }

        return loadedAssemblies.Values.ToList();
    }

    private static IMinigameRegistry BuildMinigameRegistry(
        IServiceCollection services,
        IReadOnlyCollection<IMinigameDescriptor> minigameDescriptors)
    {
        var registry = new MinigameRegistry();

        foreach (var descriptor in minigameDescriptors)
        {
            descriptor.RegisterServices(services);
            registry.Register(descriptor);
        }

        return registry;
    }

    private static IMinigameFrontendRegistry BuildFrontendRegistry(IEnumerable<IMinigameFrontendDescriptor> descriptors)
    {
        var registry = new MinigameFrontendRegistry();

        foreach (var descriptor in descriptors)
        {
            registry.Register(descriptor);
        }

        return registry;
    }

    private static void MountPluginStaticFiles(WebApplication app)
    {
        var catalog = app.Services.GetService<IPluginCatalog>();
        if (catalog is null)
        {
            return;
        }

        var seenRequestPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var plugin in catalog.Plugins)
        {
            if (plugin.StaticFiles is null)
            {
                continue;
            }

            foreach (var frontend in plugin.FrontendDescriptors)
            {
                // Match existing URL convention: /minigames/<gamekey-lowercase>/...
                var requestPath = $"/minigames/{frontend.GameKey.ToLowerInvariant()}";
                if (!seenRequestPaths.Add(requestPath))
                {
                    continue;
                }

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = plugin.StaticFiles,
                    RequestPath = requestPath,
                });
            }
        }
    }

    private static IEnumerable<TDescriptor> CreateDescriptors<TDescriptor>(IReadOnlyList<Assembly> assemblies)
        where TDescriptor : class
    {
        var descriptorTypes = assemblies
            .Where(a => a.GetName().Name?.StartsWith("Quizanchos.", StringComparison.OrdinalIgnoreCase) == true)
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t => typeof(TDescriptor).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .OrderBy(t => t.FullName)
            .ToList();

        foreach (var descriptorType in descriptorTypes)
        {
            if (Activator.CreateInstance(descriptorType) is TDescriptor descriptor)
            {
                yield return descriptor;
            }
        }
    }

    private static void AddControllers(
        WebApplicationBuilder builder,
        IReadOnlyCollection<IMinigameDescriptor> minigameDescriptors)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        string smtpHost = configuration.GetOption("EmailConfirmation:Smtp:Host");
        if (!int.TryParse(configuration.GetOption("EmailConfirmation:Smtp:Port"), out int smtpPort))
        {
            throw Util.CriticalExceptionFactory.CreateConfigException("EmailConfirmation:Smtp:Port");
        }
        string smtpUser = configuration.GetOption("EmailConfirmation:Smtp:User");
        string smtpPassword = configuration.GetOption("EmailConfirmation:Smtp:Password");

        services.AddFluentEmail(configuration.GetOption("EmailConfirmation:Smtp:FromEmail"), configuration.GetOption("EmailConfirmation:Smtp:FromName"))
            .AddSmtpSender(() => new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword)
            });

        services.AddTransient<EmailSenderService>();
        services.AddTransient<DefaultNicknameGenerator>();
        services.AddTransient<DefaultUserRegistrationService>();
        services.AddTransient<EmailConfirmationUserRegistrationService>();
        services.AddTransient<IUserPasswordUpdaterService, EmailConfirmationPasswordUpdaterService>();
        services.AddTransient<IUserRegistrationService, EmailConfirmationUserRegistrationService>();

        services.AddControllersWithViews()
            .AddJsonOptions(options => ConfigureJsonOptions(options, minigameDescriptors));
    }

    private static void ConfigureJsonOptions(
        Microsoft.AspNetCore.Mvc.JsonOptions options,
        IReadOnlyCollection<IMinigameDescriptor> minigameDescriptors)
    {
        options.JsonSerializerOptions.TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                (typeInfo) =>
                {
                    // Configure polymorphic serialization for GameMove
                    if (typeInfo.Type == typeof(Quizanchos.Core.GameMove))
                    {
                        var usedDiscriminators = new HashSet<string>(StringComparer.Ordinal);
                        typeInfo.PolymorphismOptions = new System.Text.Json.Serialization.Metadata.JsonPolymorphismOptions
                        {
                            TypeDiscriminatorPropertyName = "gameType",
                            UnknownDerivedTypeHandling = System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor
                        };

                        foreach (var descriptor in minigameDescriptors)
                        {
                            if (descriptor.MoveType == null)
                            {
                                throw new InvalidOperationException($"Minigame descriptor '{descriptor.GetType().FullName}' has null MoveType.");
                            }

                            if (!typeof(Quizanchos.Core.GameMove).IsAssignableFrom(descriptor.MoveType))
                            {
                                throw new InvalidOperationException(
                                    $"Minigame descriptor '{descriptor.GetType().FullName}' declares move type '{descriptor.MoveType.FullName}' that does not derive from {nameof(Quizanchos.Core.GameMove)}.");
                            }

                            if (string.IsNullOrWhiteSpace(descriptor.MoveDiscriminator))
                            {
                                throw new InvalidOperationException($"Minigame descriptor '{descriptor.GetType().FullName}' has empty MoveDiscriminator.");
                            }

                            if (!usedDiscriminators.Add(descriptor.MoveDiscriminator))
                            {
                                throw new InvalidOperationException($"Duplicate move discriminator '{descriptor.MoveDiscriminator}' detected during startup.");
                            }

                            typeInfo.PolymorphismOptions.DerivedTypes.Add(
                                new System.Text.Json.Serialization.Metadata.JsonDerivedType(
                                    descriptor.MoveType,
                                    descriptor.MoveDiscriminator));
                        }
                    }
                }
            }
        };
    }
}
