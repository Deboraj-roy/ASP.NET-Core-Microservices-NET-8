using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthetication();
builder.Services.AddOcelot();
var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.UseOcelot();
app.Run();
