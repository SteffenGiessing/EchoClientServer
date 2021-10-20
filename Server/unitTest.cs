using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using NUnit.Framework;
using Xunit;

namespace Server
{

    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("cid")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
    }

    public class Assignment3Tests
    {
        private const int Port = 5000;



        [Fact]
        public void Constraint_ConnectionWithoutRequest_ShouldConnect()
        {
            var client = Connect();
            Assert.True(client.Connected);
        }
        
        private static TcpClient Connect()
        {
            var client = new TcpClient();
            client.Connect("localhost", Port);
            return client;
        }

    }
}