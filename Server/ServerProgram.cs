using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestPlatform.Common;

namespace Server
{
    public class ServerProgram
    {
        public class RequestFormat
        {
            public string method { get; set; }
            public string path { get; set; }
            public string date { get; set; }
            public string body { get; set; }

        }

        static void Main(string[] args)
        {

            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            var server = new TcpListener(ipAddress, 5000);
            server.Start();
            Console.WriteLine("Server started");
            while (true)
            {
                Console.WriteLine("Listeninger");
                var client = server.AcceptTcpClient();
                
                var request = client.ReadResponse();
                Console.WriteLine($"Incoming request: {request.JsonObj()}");


                //Console.WriteLine($"Request: {request}");


                var stream = client.GetStream();
                var buffer = new byte[1024];
                var rdCnt = stream.Read(buffer);
                var message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"Client message '{message}' and the read count was {rdCnt}");

                var response = Encoding.UTF8.GetBytes(message.ToUpper());
                //var sendresponse = new HttpResponseMessage().StatusCode == HttpStatusCode.Accepted


                stream.Write(response);

            }
        }
    }

    public static partial class Util
        {
            public static string JsonObj(this object data)
            {
                return JsonSerializer.Serialize(data,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            }
            
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
                    return JsonSerializer.Deserialize<ServerProgram.RequestFormat>(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                }
            }
        }

    
}
