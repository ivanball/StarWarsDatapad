using Datapad.Api.Swapi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSwapi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();
