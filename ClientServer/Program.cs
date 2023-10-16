using System;
using ClientServer.Tcp;
using System.Threading;
using Serilog;

namespace CJTPApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogging();

            using (CJTPServer.CJTPServer server = new CJTPServer.CJTPServer(5000))
            {
                StartServer(server);
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
            // Keep the main thread alive using Console.ReadLine()
            
            Log.CloseAndFlush();
        }

        static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Logging configured");
        }

        static void StartServer(CJTPServer.CJTPServer server)
        {
            new Thread(() =>
            {
                try
                {
                    server.Start();
                }
                catch (Exception ex)
                {
                    Log.Error("Server error: {Message}", ex.Message);
                }
            }).Start();

            Log.Information("Server started");
        }

        
    }
}
