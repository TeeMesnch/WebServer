using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    public class WebSocketHandler
    {
        public static async Task WebSocketServer(IPEndPoint ip)
        {
            using Socket listener = new(
                ip.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ip);
            listener.Listen(100);

            var handler = await listener.AcceptAsync();
            while (true)
            {
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }
            }
        }
    }
}

