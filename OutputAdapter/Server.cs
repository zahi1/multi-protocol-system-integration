namespace Servers;

using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using NLog;

public class GasPressureServer
{
    /// <summary>
    /// Logger for this class.
    /// </summary>
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Configure logging subsystem using NLog.
    /// </summary>
    private void ConfigureLogging()
    {
        var config = new NLog.Config.LoggingConfiguration();

        var console = new NLog.Targets.ConsoleTarget("console")
        {
            Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
        };
        config.AddTarget(console);
        config.AddRuleForAllLevels(console);

        LogManager.Configuration = config;
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var self = new GasPressureServer();
        self.Run(args);
    }

    /// <summary>
    /// Program body.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private void Run(string[] args)
    {
        // Configure logging
        ConfigureLogging();

        // Indicate server is about to start
        log.Info("Gas Pressure Server is about to start.");

        // Start the server
        StartServer(args);
    }

    /// <summary>
    /// Starts the integrated server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private void StartServer(string[] args)
    {
        // Create web app builder
        var builder = WebApplication.CreateBuilder(args);

        // Configure the integrated server
        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.Listen(IPAddress.Loopback, 5001); // Server listens on localhost:5001
        });

        // Add and configure Swagger documentation generator
        builder.Services.AddSwaggerGen(opts =>
        {
            // Include code comments in Swagger documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        // Turn on support for Web API controllers
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                // Make enumeration values appear as strings instead of integers in OpenAPI docs
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Add CORS policies
        builder.Services.AddCors(cr =>
        {
            // Allow everything from everywhere
            cr.AddPolicy("allowAll", cp =>
            {
                cp.AllowAnyOrigin();
                cp.AllowAnyMethod();
                cp.AllowAnyHeader();
            });
        });

        // Build the server
        var app = builder.Build();

        // Enable CORS policy
        app.UseCors("allowAll");

        // Enable Swagger documentation web page
        app.UseSwagger();
        app.UseSwaggerUI();

        // Enable request routing
        app.UseRouting();

        //configure routes
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}"
        );

        // Run the server
        app.Run();
    }
}
