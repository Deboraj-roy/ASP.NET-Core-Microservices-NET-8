using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
builder.Services.AddOcelot();


app.MapGet("/", () => "Hello World!");
app.UseOcelot();
app.Run();
