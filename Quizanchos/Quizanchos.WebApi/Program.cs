using Quizanchos.WebApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Quizanchos.WebApi");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.AddAuthorizaiton();
    builder.AddApplicationServices();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSerilogRequestLogging();
    app.Configure();

    await app.SeedData(builder.Configuration);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Quizanchos.WebApi terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
