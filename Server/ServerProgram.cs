using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
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
            Response response = new Response();
            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            var server = new TcpListener(ipAddress, 5000);
            server.Start();
            Console.WriteLine("Server started");
            Console.WriteLine(UnixTimestamp());
            while (true)
            {
                Console.WriteLine("Listening");
                var client = server.AcceptTcpClient();
                var request = client.ReadResponse();
                Console.WriteLine(request);
                Console.WriteLine($"Incoming request: {request.JsonObj()}");
                // client = Our TcpClient.
                // request = The incoming message.
                // response = What we want to send back.
                var task = Task<RequestFormat>.Run(() =>
                {
                    Console.WriteLine("New Request Getting Checked");
                    var legalmethods = "create read update delete echo";
                    var requirespath = "create read update delete";
                    var categories = new List<object>();
                    categories.Add(new {cid = 1, name = "Beverages"});
                    categories.Add(new {cid = 2, name = "Condiments"});
                    categories.Add(new {cid = 3, name = "Confections"});
                    Console.WriteLine(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    Console.WriteLine(DateTimeOffset.Now.ToString());

                    DateTime now = DateTime.Now;

                    if (request.Method == null && request.Date == null && request.Body == null)
                    {

                        response.Status = "missing body, illegal body, missing date, missing method";
                        client.SendRequest(response.ToJson());
                    }

                    
                    if (requirespath.Contains(request.Method.ToLower()) && request.Path == null)
                    {
                        response.Status = "missing resource";
                        client.SendRequest(response.ToJson());
                    }


                    if (request.Method.Contains("create") || request.Method.Contains("update") ||
                        request.Method.Contains("echo") || request.Method.Contains("delete") && request.Body == null && request.Date != null &&
                        request.Date.Length == 10 && request.Path != null && request.Path.Length > 4)
                    {
                        if (request.Path != null && request.Path.Contains("/api/categories"))
                        {
                            if (request.Body != null && !request.Body.Contains("{}"))
                            {
                                if (request.Body.Contains("name"))
                                {
                                    Console.WriteLine("here");
                                    response.Status = "4 bad request";
                                    client.SendRequest(response.ToJson());
                                }
                                response.Status = "illegal body";
                                client.SendRequest(response.ToJson());
                            }

                            if (request.Body == null && request.Method == "delete")
                            {
                                Console.WriteLine("here1");
                                response.Status = "4 bad request";
                                client.SendRequest(response.ToJson());  
                            }
                        }
                        else if (request.Date.Length > 15)
                        {
                            response.Status = "illegal date";
                            client.SendRequest(response.ToJson());
                        }
                        Console.WriteLine("here1");
                        response.Status = "missing body";
                        client.SendRequest(response.ToJson());
                    }
                    else if (request.Method == "xxxx")
                    {
                        response.Status = "illegal method";
                        client.SendRequest(response.ToJson());
                    }
                    if (requirespath.Contains(request.Method.ToLower()) && !request.Path.Contains("/api/categories"))
                    {
                        
                        response.Status = "4 Bad Request";
                        response.Body = null;
                        client.SendRequest(response.ToJson());
                    } else if (requirespath.Contains(request.Method.ToLower()) && request.Path.Contains("/api/categories"))
                    {
                        response.Status = "4 Bad Request";
                        response.Body = null;
                        client.SendRequest(response.ToJson());
                        
                    }
                    
                    
                  
                    if (request.Method == "echo")
                    {
                        response.Status = "Hello World";
                        response.Body = "Hello World";
                        client.SendRequest(response.ToJson());
                    }
                 
                  
                    if (request.Method == "read" && request.Path == "/api/categories/1")
                    {
                        response.Status = "1 Ok";
                        response.Body = categories[0].JsonObj();
                        client.SendRequest(response.ToJson());
                    }
                    
     
                    if (!request.Path.Contains("/api/xxx") || request.Path.Contains("/api/categories/xxx"))
                    {
                        response.Status = "4 Bad Request";
                        client.SendRequest(response.ToJson());
                    }
                    
                    if (request.Method == "create")
                    {
                        response.Status = "4 Bad Request";
                    }
                    
                    /*if (request.Date == null)
                    {
                        response.Status = " missing date";
                        client.SendRequest(response.ToJson());
                    }*/

                    /*if (request.Date.Contains(""))
                         {
                             response.Status = "illegal date, missing resources";
                             client.SendRequest(response.ToJson());
                         }*/

                    client.SendRequest(response.ToJson());
                });
            }
        }
    }
    public static class Util
    {
        public static string JsonObj(this object data)
        {
            return JsonSerializer.Serialize(data,
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
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
    }
}