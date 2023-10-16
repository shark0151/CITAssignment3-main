using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ClientServer.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace ClientServer.Tcp
{
    public class Category
    {
        public Category() { }
        public Category(int _cid, string name)
        {
            Cid = _cid;
            Name = name;
        }
        public int Cid { get; set; }
        public string Name { get; set; }
    }
    /// <summary>
    /// Represents a CJTP (Custom JSON Transfer Protocol) server.
    /// </summary>
    public class CJTPServer : IDisposable
    {
        private static List<Category> categories = new List<Category>
        {
        new Category() { Cid = 1, Name = "Beverages" },
        new Category() { Cid = 2, Name = "Condiments" },
        new Category() { Cid = 3, Name = "Confections" }
        };
        private const int BufferSize = 1024;
        private TcpListener _tcpListener;
        private readonly object _lock;

        /// <summary>
        /// Initializes a new instance of the CJTPServer class with the specified port.
        /// </summary>
        /// <param name="port">The port on which the server will listen for incoming connections.</param>
        public CJTPServer(int port)
        {
            _lock = new object();
            _tcpListener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Starts the CJTP server and begins listening for incoming connections.
        /// </summary>
        public void Start()
        {
            _tcpListener.Start();
            // Begin accepting a client connection asynchronously.
            _tcpListener.BeginAcceptTcpClient(HandleClient, null);
        }

        /// <summary>
        /// Asynchronous callback for handling client connections.
        /// </summary>
        /// <param name="asyncResult">The result of the asynchronous operation.</param>
        private void HandleClient(IAsyncResult asyncResult)
        {
            lock (_lock) // Ensure thread safety when handling clients
            {
                var client = _tcpListener.EndAcceptTcpClient(asyncResult);
                // Continue to accept new client connections.
                _tcpListener.BeginAcceptTcpClient(HandleClient, null);

                using (NetworkStream stream = client.GetStream())
                {
                    var requestBytes = new byte[BufferSize];
                    stream.ReadTimeout = 5000;

                    try
                    {
                        var bytesRead = stream.Read(requestBytes, 0, requestBytes.Length);
                        var requestJson = Encoding.UTF8.GetString(requestBytes, 0, bytesRead);
                        var request = JsonConvert.DeserializeObject< CJTPRequest >(requestJson);

                        var response = ProcessRequest(request);
                        var responseJson = ToJson(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                        // Send the response to the client.
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        var response = new CJTPResponse();
                        response.Status = StatusCodes.BadRequest;
                        var responseJson = ToJson(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of the CJTP server by stopping the TCP listener.
        /// </summary>
        public void Dispose()
        {
            _tcpListener?.Stop();
            _tcpListener = null;
        }

        public string ToJson(object data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
        private int? GetIdFromPath(string caca)
        {
            try
            {
                return Convert.ToInt32(caca.Split('/').Last());
            }
            catch (Exception e) { return null; }
        }

        private CJTPResponse Create(CJTPRequest request)
        {
            var response = new CJTPResponse();
            try
            {
                categories.Add(new Category() { Cid = categories.Count + 1, Name = request.Body.Name });

                response.Status = StatusCodes.Created;
                response.Body = "Created";
            }
            catch (Exception ex)
            {
                response.Status = StatusCodes.BadRequest;
                Log.Error(ex.Message, ex);
            }
            return response;
        }

        

        private CJTPResponse Read(CJTPRequest request)
        {
            var response = new CJTPResponse();
            try
            {
                int? x = GetIdFromPath(request.Path);
                if (x != null)
                {
                    if (categories[x.Value] == null)
                    {

                    }
                    else
                    {
                        response.Status = StatusCodes.Ok;
                        response.Body = ToJson(categories[x.Value]);
                    }
                }
                else
                {
                    response.Status = StatusCodes.Ok;
                    response.Body = ToJson(categories);
                }

            }
            catch (Exception ex)
            {
                response.Status = StatusCodes.BadRequest;
                Log.Error(ex.Message, ex);
            }
            return response;
        }

        private CJTPResponse Update(CJTPRequest request)
        {
            var response = new CJTPResponse();
            try
            {
                int? x = GetIdFromPath(request.Path);
                if (x != null)
                {
                    categories[x.Value] = new Category() { Name = request.Body.Name };
                    response.Status = StatusCodes.Updated;
                } else
                {
                    response.Status = StatusCodes.BadRequest;
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusCodes.BadRequest;
                Log.Error(ex.Message, ex);
            }
            return response;
        }

        private CJTPResponse Delete(CJTPRequest request)
        {
            var response = new CJTPResponse();
            try
            {
                int? x = GetIdFromPath(request.Path);
                if (x != null)
                {
                    categories.RemoveAt(x.Value);
                    response.Status = StatusCodes.Ok;
                } else
                {
                    response.Status = StatusCodes.BadRequest;
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusCodes.BadRequest;
                Log.Error(ex.Message, ex);
            }
            return response;
        }

        private CJTPResponse Echo(CJTPRequest request)
        {
            var response = new CJTPResponse();
            response.Status = StatusCodes.Ok;
            response.Body = request.Body.ToString();
            return response;
        }
        /// <summary>
        /// Process a CJTP request and generate a response.
        /// </summary>
        /// <param name="request">The CJTPRequest to be processed.</param>
        /// <returns>A CJTPResponse representing the response to the request.</returns>
        private CJTPResponse ProcessRequest(CJTPRequest request)
        {
            var response = new CJTPResponse();

            if (string.IsNullOrEmpty(request.Method))
            {
                response.Status = StatusCodes.BadRequest + " missing method ";
                
            }

            if (string.IsNullOrEmpty(request.Path) && request.Method != "echo")
            {
                response.Status = response.Status + StatusCodes.BadRequest + " missing path ";
            }
            if(!request.Path.Contains("api/categories")) 
            {
                response.Status = response.Status + StatusCodes.BadRequest + " missing path ";
            }

            if (request.Date is null)
            {
                response.Status = response.Status + StatusCodes.BadRequest + " missing date ";
            }

            if(!string.IsNullOrEmpty(response.Status))
            return response;

            switch (request.Method)
            {
                case "create":

                    response = Create(request);
                    break;

                case "read":

                    response = Read(request);
                    break;

                case "update":
                    response = Update(request);
                    break;

                case "delete":

                    response = Delete(request);
                    break;

                case "echo":

                    response = Echo(request);
                    break;

                default:
                    response.Status = StatusCodes.BadRequest + " illegal method";
                    return response;
            }

            return response;
        }
    }
}