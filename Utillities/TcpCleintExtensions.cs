using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Utillities
{
    public static class TcpClientExtensions
    {
        public static void Send(this TcpClient client, string message)
        {
            client.GetStream().Write(Encoding.UTF8.GetBytes(message));
        }

        public static string Receive(this TcpClient client, int bufferSize = 1024)
        {
            //var buffer = new byte[1024];
            //var rdCnt =client.GetStream().Read(buffer);
            //return Encoding.UTF8.GetString(buffer, 0, rdCnt);

            var strm = client.GetStream();
            byte[] resp = new byte[bufferSize];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);

                } while (bytesread == bufferSize);

                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
    }
}
