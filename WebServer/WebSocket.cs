using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    public class WebSocketHandler
    {
        public static async Task RunWebsocket(IPEndPoint ip)
        {
            using Socket sock = new(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sock.Bind(ip);
            sock.Listen(100);

            var socketHandler = await sock.AcceptAsync();
            HandleTlsHandshake();
            
            while (true)
            {
                var buffer = new byte[1024];
                var received = await socketHandler.ReceiveAsync(buffer, SocketFlags.None);
                
                string request = Encoding.UTF8.GetString(buffer, 0, received);
                var key = HttpParser.GetHeader(request).wsKey;
                
                HandleWsHandshake(request, key);
                
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1)
                {
                    Console.WriteLine($"Socket server received message: \"{response.Replace(eom, "")}\"");
                    
                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await socketHandler.SendAsync(echoBytes, 0);
                    
                    Console.WriteLine($"Socket server sent acknowledgment: \"{ackMessage}\"");
                    break;
                }
            }
        }
        
        private static string HandleWsHandshake(string request, string key)
        {
            return string.Empty;
        }

        private static void HandleTlsHandshake()
        {
            
        }

        private static void ListenForEndSignal()
        {
            
        }

        private static void HandleChatMessage()
        {
            
        }
    }
}

