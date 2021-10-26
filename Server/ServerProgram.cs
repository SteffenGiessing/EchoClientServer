using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestPlatform.Common;
using Server;

namespace Server
{
    //this is a test
    //this is another test
    public class ServerProgram
    {
        private static List<Category> Categories = new()
        {
            new Category {Id = 1, Name = "Beverages"},
            new Category {Id = 2, Name = "Condiments"},
            new Category {Id = 3, Name = "Confections"}
        };

        private const int MaxPathLenght = 4;

        public class RequestFormat
        {
            public string Method { get; set; }
            public string Path { get; set; }
            public string Date { get; set; }
            public string Body { get; set; }
        }

        public class Response
        {
            public string Body { get; set; }
            public string Status { get; set; }
        }

        public class Category
        {
            [JsonPropertyName("cid")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; }
        }

        private static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }

        static void Main(string[] args)
        {
            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            var server = new TcpListener(ipAddress, 5000);
            server.Start();
            Console.WriteLine("Server started");
            Console.WriteLine(UnixTimestamp());
            var exit = false;
            while (!exit)
            {
                var response = new Response();
                Console.WriteLine("Listening");
                var client = server.AcceptTcpClient();
                var request = client.ReadResponse();
                Console.WriteLine(request);
                Console.WriteLine($"Incoming request: {request.ToJson()}");
                // client = Our TcpClient.
                // request = The incoming message.
                // response = What we want to send back.

                Console.WriteLine("New Request Getting Checked");
                Console.WriteLine(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                Console.WriteLine(DateTimeOffset.Now.ToString());

                if (string.IsNullOrEmpty(request.Method))
                {
                    response.Status += ErrorFormatter.FormatMissingMessage(nameof(request.Method));
                }

                if (request.Date == null)
                {
                    response.Status += ErrorFormatter.FormatMissingMessage(nameof(request.Date));
                }

                else if (DateTime.TryParse(request.Date, out _))
                {
                    response.Status += ErrorFormatter.FormatIllegalMessage(nameof(request.Date));
                }

                if (request.Method is Methods.Create or Methods.Read or Methods.Update or Methods.Delete)
                {
                    if (string.IsNullOrEmpty(request.Path))
                    {
                        response.Status += ErrorFormatter.FormatMissingMessage("Resource");
                    }
                    else
                    {
                        var pathVariables = request.Path.Split('/');

                        // "/api/categories/1".Split('/') == ["", "api", "categories", "1"]
                        if (pathVariables.Length is > MaxPathLenght or <= 2)
                        {
                            response.Status += ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest);
                        }
                        else if (pathVariables[1] != "api" || pathVariables[2] != "categories")
                        {
                            response.Status += ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest);
                        }
                        else if (request.Method is Methods.Update or Methods.Delete)
                        {
                            if (pathVariables.Length < MaxPathLenght)
                            {
                                response.Status = ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest).Trim();
                                client.SendRequest(response.ToJson());
                                continue;
                            }

                            if (!int.TryParse(pathVariables[3], out _))
                            {
                                response.Status += ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest);
                            }
                        }

                        else if (request.Method is Methods.Read)
                        {
                            if (pathVariables.Length == MaxPathLenght && !int.TryParse(pathVariables[3], out _))
                            {
                                response.Status = ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest)
                                    .Trim();
                                client.SendRequest(response.ToJson());
                                continue;
                            }
                        }

                        if (request.Method is Methods.Create && pathVariables.Length > 3)
                        {
                            response.Status = ErrorFormatter.FormatGenericMessage(Status.BadReq, StatusMessages.BadRequest).Trim();
                            client.SendRequest(response.ToJson());
                            continue;
                        }
                    }
                }

                if (request.Method is Methods.Create or Methods.Update or Methods.Echo)
                {
                    if (string.IsNullOrEmpty(request.Body))
                    {
                        response.Status += ErrorFormatter.FormatMissingMessage(nameof(request.Body));
                    }

                    else if (request.Method != Methods.Echo && !request.Body.JsonTryParse<Category>(out _))
                    {
                        response.Status += ErrorFormatter.FormatIllegalMessage(nameof(request.Body));
                    }
                }

                if (string.IsNullOrEmpty(response.Status))
                {
                    switch (request.Method)
                    {
                        case Methods.Create:
                            break;
                        case Methods.Read:
                            response = HandleRead(request);
                            break;
                        case Methods.Update:
                            response = HandleUpdate(request);
                            break;
                        case Methods.Delete:
                            break;
                        case Methods.Echo:
                            response.Body = request.Body;
                            break;
                        case Methods.Exit:
                            exit = true;
                            break;
                        default:
                            response.Status += ErrorFormatter.FormatIllegalMessage(nameof(request.Method));
                            break;
                    }

                }

                response.Status = response.Status?.Trim();
                if (!exit)
                    client.SendRequest(response.ToJson());
            }

            server.Stop();
        }

        private static int GetIdFromPath(RequestFormat request)
        {
            var pathVariables = request.Path.Split('/');
            return pathVariables.Length == MaxPathLenght ? int.Parse(pathVariables[3]) : -1;
        }

        private static Response HandleUpdate(RequestFormat request)
        {
            var id = GetIdFromPath(request);
            var category = Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return new Response
                {
                    Status = "5 Not Found"
                };
            }

            var updatedCategory = request.Body.FromJson<Category>();

            category.Name = updatedCategory.Name;
            
            return new Response
            {
                Status = "3 Updated",
                Body = category.ToJson()
            };
        }
        

        private static Response HandleRead(RequestFormat request)
        {
            var id = GetIdFromPath(request);

            if (id != -1)
            {
                var category = Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    return new Response
                    {
                        Status = "5 Not Found"
                    };
                }

                return new Response
                {
                    Status = "1 Ok",
                    Body = category.ToJson()
                };
            }

            return new Response
            {
                Status = "1 Ok",
                Body = Categories.ToJson()
            };
        }
    }

    public static class Util
    {
        public static bool JsonTryParse<T>(this string data, out T json)
        {
            json = default;

            try
            {
                json = data.FromJson<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void Send(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }

        public static void SendRequest(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }

        //Note that this method takes RequestFormat -> From ServerProgram - shall be changed for future i think.
        public static ServerProgram.RequestFormat ReadResponse(this TcpClient client)
        {
            var strm = client.GetStream();
            //strm.ReadTimeout = 250;
            byte[] resp = new byte[2048];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);
                } while (bytesread == 2048);

                var responseData = Encoding.UTF8.GetString(memStream.ToArray());
                // Deseralize the object into RequestFormat
                return JsonSerializer.Deserialize<ServerProgram.RequestFormat>(responseData,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            }
        }

        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data,
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }

        public static T FromJson<T>(this string element)
        {
            return JsonSerializer.Deserialize<T>(element,
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }
    }
}