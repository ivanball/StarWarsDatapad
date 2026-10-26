using Datapad.Api.Characters;
using Datapad.Api.Swapi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SwapiUnavailableExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddSwapi(builder.Configuration);
builder.Services.AddScoped<CharacterService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapCharacterEndpoints();

// Unknown /api routes get a 404 problem response; everything else falls through to the Blazor client's router.
app.MapFallback("/api/{**path}", () => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found"));
app.MapFallbackToFile("index.html");

app.Run();
