using System.Text;

namespace WebServer
{
    public class Endpoints
    {
        private static async Task<byte[]> ReadFileAsync(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public static Task<byte[]> IndexHtml()
        {
            return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Index/index.html");
        }

        public static Task<byte[]> IndexJs()
        {
            return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Index/main.js");
        }

        public static Task<byte[]> IndexCss()
        {
            return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Index/style.css");
        }

        public static byte[] Echo(string request)
        {
            try
            {
                if (HttpParser.GetDomain(request).Length > 5)
                {
                    var text = HttpParser.GetDomain(request).Substring(6);
                    return Encoding.UTF8.GetBytes(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return Array.Empty<byte>();
        }

        public class File
        {
            public static async Task Create(string fileName, string content)
            {
                try
                {
                    await System.IO.File.Create(fileName).DisposeAsync();
                    await System.IO.File.WriteAllTextAsync(fileName, content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            public static async Task<byte[]> GetContent(string fileName)
            {
                try
                {
                    using var readStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.Open,
                        FileAccess.Read);
                    byte[] buffer = new byte[readStream.Length];
                    await readStream.ReadAsync(buffer, 0, buffer.Length);
                    return buffer;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            public static void Compress(string fileName)
            {
                using var fileStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.OpenOrCreate,
                    FileAccess.ReadWrite);
            }
        }

        static void Video()
        {
            Console.WriteLine("Video started...");
        }

        public class Chat
        {
            public static Task<byte[]> ChatCss()
            {
                return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.css");
            }

            public static Task<byte[]> ChatJs()
            {
                return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.js");
            }

            public static Task<byte[]> ChatHtml()
            {
                return ReadFileAsync("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.html");
            }

            public static void HandleWebSocket()
            {
                Console.WriteLine("WebSocket started...");
            }

            public static (string userName, string message) DisplayContent(string content)
            {
                string userName = string.Empty;
                string message = string.Empty;

                try
                {
                    userName = content.Substring("{\"username\":\"".Length).Split('"')[0];
                    var unparsedMessage = content.Split(",")[1].Substring(" message: \"".Length);
                    message = unparsedMessage.Split('"')[0];

                    return (userName, message);
                }
                catch (Exception)
                {
                    Console.WriteLine();
                }

                return (userName, message);
            }
        }
    }
}
