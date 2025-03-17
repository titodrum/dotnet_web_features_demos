using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry logging

// builder.Logging.ClearProviders();
// builder.Logging.AddOpenTelemetry(x =>
// {
//     x.SetResourceBuilder(
//         ResourceBuilder.CreateDefault()
//         .AddService("WeatherForecast")
//         .AddEnvironmentVariableDetector()
//         .AddAttributes(new Dictionary<string, object>
//         {
//             { "key1", "value1" },
//             { "key2", "value2" },
//         })
//         .AddAttributes(new Dictionary<string, object>
//         {
//             { "key3", "value3" },
//             { "key4", "value4" },
//         })
//         .AddAttributes(new Dictionary<string, object>
//         {
//             { "key5", "value5" },
//             { "key6", "value6" },
//         })
//     );

//     x.IncludeScopes = true;
//     x.IncludeFormattedMessage = true;

//     //x.AddConsoleExporter();
//     x.AddOtlpExporter(a =>
//     {
//         a.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
//         a.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
//         a.Headers = "X-Seq-ApiKey=x3SS5lFDHpWnvoIw7ZkA"; //x3SS5lFDHpWnvoIw7ZkA
//     });
// });

// Serilog

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry(a =>
    {
        a.Endpoint = "http://localhost:5341/ingest/otlp/v1/logs";
        a.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;
        a.Headers = new Dictionary<string, string>
        {
            ["X-Seq-ApiKey"] = "x3SS5lFDHpWnvoIw7ZkA"//x3SS5lFDHpWnvoIw7ZkA
        };
        a.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = "WeatherForecast",
        };
    })
    .CreateLogger();
builder.Services.AddSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    logger.LogInformation("Generated weather forecast");
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
