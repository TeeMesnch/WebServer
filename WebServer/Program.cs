using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    class Server
    {
        static async Task Main(string[] args)
        {
            await RunServer();
        }

        static async Task RunServer()
        {
            const int port = 4200;
            var ip = new IPEndPoint(IPAddress.Loopback, port);

            TcpListener server = new TcpListener(ip);
            server.Start();

            Console.WriteLine($"starting server (port: {port}) (ip: {ip})\n");

            while (true)
            {
                using var client = server.AcceptTcpClientAsync();
                var stream = client.Result.GetStream();
                
                byte[] buffer = new byte[1024];
                
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                Console.WriteLine($"request: {request}");
            }
        }
    }
}

