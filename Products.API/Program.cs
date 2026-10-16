using Microsoft.OpenApi;
using Products.API.Middleware;
using Products.Application;
using Products.Infrastructure;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap logger: keeps startup failures observable before the host is built.
// UseSerilog() below re-configures (and freezes) this same logger from appsettings.json.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

var log = Log.ForContext<Program>();

try
{
    log.Information("Starting the Products API");

    builder.Host.UseSerilog((context, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Products API",
            Version = "v1",
            Description = "Products Web API built with Clean Architecture, CQRS, MediatR and EF Core."
        });
    });

    var app = builder.Build();

    // Registered first so it measures the whole pipeline and observes the final status code.
    // Unexpected exceptions are converted by ExceptionHandlingMiddleware before they reach
    // this middleware, so each exception is only logged once.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsedMs, exception) =>
            exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                ? LogEventLevel.Error
                : LogEventLevel.Information;
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Products API v1"));
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    log.Information("The Products API is ready to accept requests");
    app.Run();
    log.Information("The Products API stopped cleanly");
    return 0;
}
catch (Exception exception)
{
    log.Fatal(exception, "The Products API terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
