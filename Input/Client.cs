using Grpc.Net.Client;
using NLog;
using Services;

/// <summary>
/// GasPressure Input Client for interacting with GasContainerService.
/// </summary>
class GasPressureInputClient
{
    /// <summary>
    /// Logger for this class.
    /// </summary>
    private Logger mLog = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Configures logging subsystem.
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
    /// Program logic.
    /// </summary>
    private void Run()
    {
        // Configure logging
        ConfigureLogging();

        // Initialize random number generator
        var rnd = new Random();

        // Main loop to reconnect in case of connection errors
        while (true)
        {
            try
            {
                // Connect to the gRPC server and get the service proxy
                var channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"); // Update to your server's address
                var gasService = new GasContainerService.GasContainerServiceClient(channel);

                // Log initial message
                mLog.Info("GasPressure Input Client started.");

                while (true)
                {
                    // Check if the container is destroyed
                    var isDestroyed = gasService.IsDestroyed(new Empty()).Value;

                    if (isDestroyed)
                    {
                        mLog.Warn("Gas container is destroyed. Waiting for reset...");
                        Thread.Sleep(5000); // Wait before checking again
                        continue;
                    }

                    // Get the current pressure
                    var pressureResponse = gasService.GetPressure(new Empty());
                    double currentPressure = pressureResponse.Value;

                    mLog.Info($"Current Pressure: {currentPressure}");

                    // Add mass if the pressure is below a certain threshold
                    if (currentPressure < 100)
                    {
                        double massToAdd = rnd.Next(1, 5); // Random mass between 1 and 5 units
                        gasService.IncreaseMass(new DoubleMsg { Value = massToAdd });

                        mLog.Info($"Successfully requested to add mass of {massToAdd} units.");
                    }
                    else
                    {
                        mLog.Info("Pressure is above the safe limit. No mass added.");
                    }

                    // Sleep before the next iteration
                    Thread.Sleep(2000 + rnd.Next(1000));
                }
            }
            catch (Exception e)
            {
                // Log exceptions and wait before restarting the main loop
                mLog.Warn(e, "Unhandled exception caught. Restarting main loop...");

                // Prevent console spamming
                Thread.Sleep(5000);
            }
        }
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args)
    {
        var client = new GasPressureInputClient();
        client.Run();
    }
}
