using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Utillities
{
    public class NetworkClient
    {
        private TcpClient _client;
        private int _bufferSize = 2048;

        public NetworkClient()
        {
            _client = new TcpClient();
        }

        public NetworkClient(TcpClient client)
        {
            _client = client;
        }


        public void Connect(string ip, int port)
        {
            _client.Connect("localhost", 5000);
        }

        public void Write(string msg)
        {
            _client.GetStream().Write(Encoding.UTF8.GetBytes(msg));
        }

        public string Read()
        {
            //var buffer = new byte[1024];
            //var rdCnt = _client.GetStream().Read(buffer);
            //return Encoding.UTF8.GetString(buffer, 0, rdCnt);

            var strm = _client.GetStream();
            byte[] resp = new byte[_bufferSize];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);

                } while (bytesread == _bufferSize);

                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
    }
}
