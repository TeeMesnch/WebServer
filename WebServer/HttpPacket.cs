using System.Text;

namespace WebServer
{
    public class HttpProtocol
    {

        public class Builder
        {
            public static byte[] BuildResponse(byte[] statusLine, IEnumerable<byte[]> headers, byte[] body)
            {
                List<byte> response = new();
                response.AddRange(statusLine);

                foreach (var header in headers)
                    response.AddRange(header);

                response.AddRange(Encoding.UTF8.GetBytes("\r\n"));
                response.AddRange(body);
                return response.ToArray();
            }
        }

        public class StatusLine
        {
            public static readonly byte[] SwitchProtocol = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\n");
            public static readonly byte[] Ok = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n");
            public static readonly byte[] Created = Encoding.UTF8.GetBytes("HTTP/1.1 201 Created\r\n");
            public static readonly byte[] NotFound = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n");
            public static readonly byte[] RequestTimeout = Encoding.UTF8.GetBytes("HTTP/1.1 408 Request Time-out\r\n");

            public static byte[] Custom(int code, string message)
            {
                return Encoding.UTF8.GetBytes($"HTTP/1.1 {code} {message}\r\n");
            }
        }

        public class HttpHeader
        {
            public static readonly byte[] ContentTypeText = Encode("Content-Type: text/plain\r\n");
            public static readonly byte[] ContentTypeHtml = Encode("Content-Type: text/html\r\n");
            public static readonly byte[] ContentTypeJs = Encode("Content-Type: text/javascript\r\n");
            public static readonly byte[] ContentTypeCss = Encode("Content-Type: text/css\r\n");
            public static readonly byte[] ContentTypeJson = Encode("Content-Type: application/json\r\n");
            public static readonly byte[] ContentTypeTextStream = Encode("Content-Type: text/event-stream\r\n");
            public static readonly byte[] ContentTypeVideoMp4 = Encode("Content-Type: video/mp4\r\n");

            public static readonly byte[] ConnectionClose = Encode("Connection: close\r\n");
            public static readonly byte[] ConnectionKeepAlive = Encode("Connection: keep-alive\r\n");
            public static readonly byte[] ConnectionUpgradeWs = Encode("Connection: upgrade\r\n");
            
            public static readonly byte[] ContentEncodingGzip = Encode("Content-Encoding: gzip\r\n");
            
            public static readonly byte[] Date = Encode($"Date: {DateTime.Now}\r\n");

            public static readonly byte[] UpgradeWs = Encode("Upgrade: websocket\r\n");
            public static readonly byte[] WsProtocol = Encode("Sec-WebSocket-Protocol: chat\r\n");
            
            public static readonly byte[] RetryAfter = Encode($"Retry-After: {Server.RetryAfter}\r\n");
            
            public static byte[] AddCustomHeader(string name, string value)
            {
                return Encoding.UTF8.GetBytes($"{name}: {value}");
            }
            
            public static byte[] ContentLength(int length)
            {
                return Encode($"Content-Length: {length}\r\n");
            }

            public static byte[] WebSocketAccept(string accept)
            {
                return Encoding.UTF8.GetBytes($"Sec-WebSocket-Accept: {accept}\r\n");
            }
            
            static byte[] Encode(string text)
            {
                return Encoding.UTF8.GetBytes(text);
            }
        }
    }
}