using Quizanchos.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddAuthorizaiton();
builder.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Configure();

await app.SeedData(builder.Configuration);

app.Run();
