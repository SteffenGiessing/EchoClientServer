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
using System.Xml.Schema;
using Microsoft.VisualStudio.TestPlatform.Common;

namespace Server
{
    //this is a test
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
            public string Status { get; set; }
            public string Body { get; set; }
        }
        public class Category
        {
            [JsonPropertyName("cid")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
        } 
        static void Main(string[] args)
        {
            Response response = new Response();
            
            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            var server = new TcpListener(ipAddress, 5000);
            server.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                Console.WriteLine("Listening");
                var client = server.AcceptTcpClient();
                
                var request = client.ReadResponse();
                Console.WriteLine($"Incoming request: {request.JsonObj()}");
                
                // client = Our TcpClient.
                // request = The incoming message.
                // response = What we want to send back.
                MessageChecker(client, request, response);
                
            }
        }
        private static void MessageChecker(TcpClient client, RequestFormat request, Response response)
        {
            var categories = new List<object>();
            categories.Add(new {cid=1, name="Beverages"});
            categories.Add(new {cid=2, name="Condiments"});
            categories.Add(new {cid=3, name="Confections"});
            
            if (request.Path == "/api/categories/1")
            {
                response.Status = "1 Ok";
                response.Body = categories[0].JsonObj();
            }
            client.SendRequest(response.ToJson());
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
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
     
    }
}
