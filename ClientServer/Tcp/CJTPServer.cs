using ClientServer.Protocol;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CJTPServer
{
    public class Category
    {
        public int Cid { get; set; }
        public string Name { get; set; }
    }

    public class CJTPRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        [JsonIgnore]
        public BodyData Body { get { return JsonConvert.DeserializeObject<BodyData>(GarbageBody); } }

        [JsonProperty("body")]
        public string GarbageBody { get; set; }
        public class BodyData
        {
            public BodyData() { }
            [JsonProperty("cid")]
            public string Cid { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }

    public class CJTPResponse
    {
        public string Status { get; set; }
        public object Body { get; set; }
    }

    public class CJTPServer : IDisposable
    {
        private TcpListener _tcpListener;
        private List<Category> _categories;
        private volatile bool _isRunning;

        public CJTPServer(int port)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _categories = new List<Category>
            {
                new Category { Cid = 1, Name = "Beverages" },
                new Category { Cid = 2, Name = "Condiments" },
                new Category { Cid = 3, Name = "Confections" }
            };
        }

        public void Start()
        {
            _tcpListener.Start();
            _isRunning = true;

            while (_isRunning)
            {
                TcpClient client = null;
                try
                {
                    client = _tcpListener.AcceptTcpClient();
                }
                catch (SocketException se)
                {
                    // Handle exceptions (e.g., log or ignore)
                    Log.Error(se, "SocketException while accepting client.");
                }

                if (client != null)
                {
                    Task.Run(async () => await HandleClientAsync(client));
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _tcpListener.Stop();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                using (NetworkStream stream = client.GetStream())
                {
                    try
                    {
                        var requestJson = await ReadRequestAsync(stream);
                        var request = JsonConvert.DeserializeObject<CJTPRequest>(requestJson);
                        if (request is null)
                            throw new Exception(request.ToString());
                        var response = ProcessRequest(request);
                        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);

                        await WriteResponseAsync(stream, responseJson);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error while handling client request.");
                        var response = new CJTPResponse();
                        response.Status = StatusCodes.BadRequest + " illegal body ";
                        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);

                        await WriteResponseAsync(stream, responseJson);
                    }
                }
            }
        }

        private async Task<string> ReadRequestAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private async Task WriteResponseAsync(NetworkStream stream, string responseJson)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        public void Dispose()
        {
            _tcpListener.Stop();
        }

        private CJTPResponse ProcessRequest(CJTPRequest request)
        {
            var response = new CJTPResponse();

            if (string.IsNullOrEmpty(request.Method))
            {
                response.Status = StatusCodes.BadRequest + " missing method ";
            }
            if (string.IsNullOrEmpty(request.Path) || !request.Path.Contains("api/categories"))
            {
                response.Status = response.Status + StatusCodes.BadRequest + " missing resource ";
            }
            if (request.Date is null)
            {
                response.Status = response.Status + StatusCodes.BadRequest + " missing date ";
            }
            else
            {
                try
                {
                    var x = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(request.Date));
                }
                catch
                {
                    response.Status = response.Status + StatusCodes.BadRequest + " illegal date ";
                }
            }

            if (!string.IsNullOrEmpty(response.Status))
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
                if(string.IsNullOrEmpty(request.Body.Name))
                { throw new Exception("Where's the name?"); }
                _categories.Add(new Category() { Cid = _categories.Count + 1, Name = request.Body.Name });

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
                    if (_categories[x.Value] == null)
                    {

                    }
                    else
                    {
                        response.Status = StatusCodes.Ok;
                        response.Body = System.Text.Json.JsonSerializer.Serialize(_categories[x.Value-1]);
                    }
                }
                else
                {
                    response.Status = StatusCodes.Ok;
                    response.Body = System.Text.Json.JsonSerializer.Serialize(_categories);
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
                    _categories[x.Value] = new Category() { Name = request.Body.Name };
                    response.Status = StatusCodes.Updated;
                }
                else
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
                    _categories.RemoveAt(x.Value);
                    response.Status = StatusCodes.Ok;
                }
                else
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
    }
}
