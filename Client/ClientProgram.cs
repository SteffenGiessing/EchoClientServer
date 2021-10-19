﻿using System;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {

            var client = new TcpClient();

            client.Connect("localhost", 5000);

            var stream = client.GetStream();

            var message = "hello";

            var msgBytes = Encoding.UTF8.GetBytes(message);

            stream.Write(msgBytes);

            var buffer = new byte[1024];

            var rdCnt = stream.Read(buffer);

            var response = Encoding.UTF8.GetString(buffer, 0, rdCnt);

            Console.WriteLine($"Server response '{response}' and the read count was {rdCnt}");

        }
    }
}
