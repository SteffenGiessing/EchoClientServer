using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server
{
    class ServerProgram
    {/*THis iS a new comment dsadsada*/
        static void Main(string[] args)
        {
            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            var server = new TcpListener(ipAddress, 5000);
            server.Start();
            Console.WriteLine("Server started");
    
            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("Client accepted");

                var stream = client.GetStream();

                var buffer = new byte[1024];

                var rdCnt = stream.Read(buffer);

                var message = Encoding.UTF8.GetString(buffer, 0, rdCnt);

                Console.WriteLine($"Client message '{message}' and the read count was {rdCnt}");

                var response = Encoding.UTF8.GetBytes(message.ToUpper());

                stream.Write(response);

            }
        }

    }
}
