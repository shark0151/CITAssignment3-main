using System;
using System.Net.Sockets;
using System.Text;
using ClientServer.Protocol;
using Newtonsoft.Json;
using Serilog;

namespace ClientServer.Tcp
{
/// <summary>
    /// Represents a client for the Custom JSON Transfer Protocol (CJTP).
    /// </summary>
    public class CJTPClient : IDisposable
    {
        private TcpClient _tcpClient;
        private const int BufferSize = 1024;

        /// <summary>
        /// Initializes a new instance of the CJTPClient class and connects to the specified server.
        /// </summary>
        /// <param name="serverAddress">The address of the CJTP server.</param>
        /// <param name="port">The port number on which the server is listening.</param>
        public CJTPClient(string serverAddress, int port)
        {
            _tcpClient = new TcpClient(serverAddress, port);
        }

        /// <summary>
        /// Sends a CJTP request to the server and receives the response.
        /// </summary>
        /// <param name="request">The CJTPRequest to be sent to the server.</param>
        /// <returns>A CJTPResponse representing the server's response, or null in case of errors.</returns>
        public CJTPResponse SendRequest(CJTPRequest request)
        {
            try
            {
                using (NetworkStream stream = _tcpClient.GetStream())
                {
                    var requestJson = JsonConvert.SerializeObject(request);
                    var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    // Send the request to the server.
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    var responseBytes = new byte[BufferSize];
                    var bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
                    var responseJson = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

                    try
                    {
                        // Deserialize the response JSON into a CJTPResponse.
                        return JsonConvert.DeserializeObject<CJTPResponse>(responseJson);
                    }
                    catch (JsonException ex)
                    {
                        Log.Error("JSON Error: {0}", ex.Message);
                        return null;
                    }
                }
            }
            catch (SocketException ex)
            {
                // Handle network errors and log the error message.
                Log.Error("Network Error: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Disposes of the CJTP client by closing the TCP connection.
        /// </summary>
        public void Dispose()
        {
            _tcpClient?.Close();
            _tcpClient = null;
        }
    }
}