using System;
using System.Threading;
using Serilog;
using ClientServer.Protocol;
using ClientServer.Tcp;

namespace Server
{
    internal class Program
    {
        private static CJTPServer _cjtpServer;
        private static CJTPClient _cjtpClient;
    
        public static void Main(string[] args)
        {
            ConfigureLogging();

            using (_cjtpServer = new CJTPServer(5000))
            {
                StartServer();
                Thread.Sleep(999999999);
            }
        
            Log.CloseAndFlush();
        }

        private static void ConfigureLogging()
        {
            // Configure logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        
            Log.Information("Logging configured");
        }

        private static void StartServer()
        {
            // Start the server on a separate thread.
            new Thread(_cjtpServer.Start).Start();
        
            Log.Information("Server started");
        }

        
    }}