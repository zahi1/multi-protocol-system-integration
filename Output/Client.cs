using System;
using System.Net.Http;
using System.Threading;
using NLog;
namespace OutputNamespace
{
    /// <summary>
    /// Client for managing pressure in a gas container.
    /// </summary>
    class Client
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialize random number generator.
        /// </summary>
        private Random rnd = new Random();

        /// <summary>
        /// Configures the logging subsystem.
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
        /// Program body.
        /// </summary>
        private void Run()
        {
            // Configure logging
            ConfigureLogging();

            // Run everything in a loop to recover from connection errors
            while (true)
            {
                try
                {
                    // Connect to the server, get service client proxy
                    var gasContainer = new OutputClient("http://127.0.0.1:5001", new HttpClient());

                    // Main loop to check and adjust pressure
                    while (true)
                    {
                        // Check if the container is destroyed before performing operations
                        if (!gasContainer.IsContainerDestroyed())
                        {
                            // Get the current pressure from the server
                            double currentPressure = gasContainer.GetPressure();
                            mLog.Info($"Current pressure: {currentPressure}");

                            // If pressure is above 150, attempt to reduce it by removing mass
                            if (currentPressure > 100)
                            {
                                double massToRemove = rnd.Next(1, 5);
                                mLog.Info($"Attempting to remove {massToRemove} units of mass.");
                                gasContainer.DecreaseMass(massToRemove);
                                mLog.Info($"Successfully removed {massToRemove} units of mass.");
                            }
                            else
                            {
                                mLog.Info("Pressure is within safe limits, no mass removed.");
                            }

                            // Wait before generating new values
                            Thread.Sleep(2000 + rnd.Next(1500));
                        }
                        else
                        {
                            mLog.Warn("Container is destroyed. Stopping further operations.");
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log any exception to console
                    mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");

                    // Prevent console spamming
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            var self = new Client();
            self.Run();
        }
    }
}
