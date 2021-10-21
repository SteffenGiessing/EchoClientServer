using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class ClientProgram
    {
        static readonly HttpClient HttpClient = new HttpClient();
        
        static void Main(string[] args)
        {
            
            var client = new TcpClient();

            client.Connect("localhost", 5000);

            var stream = client.GetStream();

            var message = Console.ReadLine();

            var msgBytes = Encoding.UTF8.GetBytes(message);

            stream.Write(msgBytes, 0, msgBytes.Length);

            var buffer = new byte[1024];

            var rdCnt = stream.Read(buffer);

            var response = Encoding.UTF8.GetString(buffer, 0, rdCnt);

            Console.WriteLine($"Server response '{response}' and the read count was {rdCnt}");
            
        }
    }
}
