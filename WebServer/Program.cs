using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace WebServer
{
    public class Server
    {
        static X509Certificate2 serverCertificate;
        public const int RetryAfter = 60;
        private const bool LocalHost = true;
        private const bool Debug = false;

        static async Task Main(string[] args)
        {
            DisplayUsage();

            // TESTING DIRECTORY
            Environment.CurrentDirectory = "/Users/jonathan/Desktop/test";

            string certificatePath = "/Users/jonathan/Desktop/localhost.pfx";
            string certificatePassword = Console.ReadLine();
            
            if (certificatePassword == string.Empty)
            {
                throw new NullReferenceException("Certificate password is required");
            }
            
            await RunServer(certificatePath, certificatePassword);
        }

        static async Task RunServer(string certificatePath, string certificatePassword)
        {
            const int port = 4200;
            var ip = new IPEndPoint(IPAddress.Loopback, port);

            if (!LocalHost)
            {
                var hostName = Dns.GetHostName();
                IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);

                IPAddress localIpAddress =
                    localhost.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (localIpAddress == null)
                    throw new Exception("No IPv4 address found for local host.");

                ip = new IPEndPoint(localIpAddress, port);
            }


            serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);

            TcpListener server = new TcpListener(ip);
            server.Start();

            Console.WriteLine($"\nstarting server (address: {ip})\n");

            while (true)
            {
                var client = server.AcceptTcpClientAsync().GetAwaiter().GetResult();
                await ProcessClient(client);
            }
        }

        static async Task ProcessClient(TcpClient client)
        {
            SslStream sslStream = new SslStream(client.GetStream(), false);

            try
            {
                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate,
                    clientCertificateRequired: false,
                    enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13,
                    checkCertificateRevocation: true
                );

                if (Debug)
                {
                    DisplaySecurityLevel(sslStream);
                    DisplaySecurityServices(sslStream);
                    DisplayCertificateInformation(sslStream);
                }

                var request = await ProcessMessage(sslStream);
                
                if (HttpParser.GetDomain(request) == "/")
                {
                    var indexHtmlPacket = await RouteIndexHtml();
                    
                    await sslStream.WriteAsync(indexHtmlPacket, 0, indexHtmlPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/main.js")
                {
                    var indexJsPacket = await RouteIndexJs();
                    
                    await sslStream.WriteAsync(indexJsPacket, 0, indexJsPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/style.css")
                {
                    var indexCssPacket = await RouteIndexCss();
                    
                    await sslStream.WriteAsync(indexCssPacket, 0, indexCssPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/chat")
                {
                    var chatHtmlPacket = await RouteChatHtml();
                    
                    await sslStream.WriteAsync(chatHtmlPacket, 0, chatHtmlPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/chat.js")
                {
                    var chatJsPacket = await RouteChatJs();
                    
                    await sslStream.WriteAsync(chatJsPacket, 0, chatJsPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/chat.css")
                {
                    var chatCssPacket = await RouteChatCss();
                    
                    await sslStream.WriteAsync(chatCssPacket, 0, chatCssPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/messages")
                {
                    var responsePacket = RouteMessages(request);
                    
                    await sslStream.WriteAsync(responsePacket, 0, responsePacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/echo/")
                {
                    var echoPacket = RouteEcho(request);
                    
                    await sslStream.WriteAsync(echoPacket, 0 , echoPacket.Length);
                }
                else if (HttpParser.GetDomain(request) == "/file/create")
                {
                    var createFilePacket= await RouteCreateFile(request);
                    
                    await sslStream.WriteAsync(createFilePacket, 0, createFilePacket.Length);
                }
                else
                {
                    var notFoundPacket= RouteNotFound(request);
                    
                    await sslStream.WriteAsync(notFoundPacket, 0 , notFoundPacket.Length);
                }
            }
            catch (AuthenticationException)
            {
                sslStream.Close();
                client.Close();
            }
            finally
            {
                await sslStream.DisposeAsync();
                sslStream.Close();
                client.Close();
            }
        }

        static async Task<byte[]> RouteIndexHtml()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;
            
            var indexHtmlBody = await Endpoints.IndexHtml();
                
            var indexHtmlHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeHtml,
                HttpProtocol.HttpHeader.ContentLength(indexHtmlBody.Length),
                HttpProtocol.HttpHeader.ConnectionKeepAlive
            };
            
            var indexHtmlPacket = HttpProtocol.Builder.BuildResponse(statusCode, indexHtmlHeaders, indexHtmlBody);

            return indexHtmlPacket;
        }

        static async Task<byte[]> RouteIndexJs()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;

            var indexJsBody = await Endpoints.IndexJs();

            var indexJsHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeJs,
                HttpProtocol.HttpHeader.ContentLength(indexJsBody.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var indexJsPacket = HttpProtocol.Builder.BuildResponse(statusCode, indexJsHeaders, indexJsBody);
            
            return indexJsPacket;
        }
        
        static async Task<byte[]> RouteIndexCss()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;
            
            var indexCssBody = await Endpoints.IndexCss();

            var indexCssHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeCss,
                HttpProtocol.HttpHeader.ContentLength(indexCssBody.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var indexCssPacket = HttpProtocol.Builder.BuildResponse(statusCode, indexCssHeaders, indexCssBody);

            return indexCssPacket;
        }

        static byte[] RouteEcho(string request)
        {
            var echoStatusCode = HttpProtocol.StatusLine.Ok;
            var echoBody = Endpoints.Echo(request);

            var echoHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeText,
                HttpProtocol.HttpHeader.ContentLength(echoBody.Length),
                HttpProtocol.HttpHeader.ConnectionKeepAlive
            };
            
            var echoPacket = HttpProtocol.Builder.BuildResponse(echoStatusCode, echoHeaders, echoBody);
            
            return echoPacket;
        }

        static byte[] RouteNotFound(string request)
        {
            var notFoundStatusCode = HttpProtocol.StatusLine.NotFound;
            var notFoundBody = Array.Empty<byte>();

            var notFoundHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.RetryAfter,
                HttpProtocol.HttpHeader.ContentLength(0),
            };
            
            var notFoundPacket = HttpProtocol.Builder.BuildResponse(notFoundStatusCode, notFoundHeaders, notFoundBody);
            
            return notFoundPacket;
        }

        static async Task<byte[]> RouteChatHtml()
        {
            var chatHtmlStatusCode = HttpProtocol.StatusLine.Ok;
            var chatHtmlBody = await Endpoints.Chat.ChatHtml();

            var chatHtmlHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeHtml,
                HttpProtocol.HttpHeader.ContentLength(chatHtmlBody.Length),
                HttpProtocol.HttpHeader.ConnectionKeepAlive
            };
            
            var chatHtmlPacket =  HttpProtocol.Builder.BuildResponse(chatHtmlStatusCode, chatHtmlHeaders, chatHtmlBody);

            return chatHtmlPacket;
        }

        static async Task<byte[]> RouteChatJs()
        {
            var chatJsStatusCode = HttpProtocol.StatusLine.Ok;
            var chatJsBody = await Endpoints.Chat.ChatJs();

            var chatJsHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeJs,
                HttpProtocol.HttpHeader.ContentLength(chatJsBody.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var chatJsPacket = HttpProtocol.Builder.BuildResponse(chatJsStatusCode, chatJsHeaders, chatJsBody);

            return chatJsPacket;
        }

        static async Task<byte[]> RouteChatCss()
        {
            var chatCssStatusCode = HttpProtocol.StatusLine.Ok;
            var chatCssBody = await Endpoints.Chat.ChatCss();

            var chatCssHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeCss,
                HttpProtocol.HttpHeader.ContentLength(chatCssBody.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var chatCssPacket = HttpProtocol.Builder.BuildResponse(chatCssStatusCode, chatCssHeaders, chatCssBody);

            return chatCssPacket;
        }
        
        static async Task<byte[]> RouteCreateFile(string request)
        {
            var createStatusCode = HttpProtocol.StatusLine.Created;
            var createBody = Array.Empty<byte>();

            var createHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentLength(0),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var createPacket = HttpProtocol.Builder.BuildResponse(createStatusCode, createHeaders, createBody);

            if (HttpParser.GetDomain(request).StartsWith("/file/create/"))
            {
                int index = "/file/create/".Length;
                var name = HttpParser.GetDomain(request).Substring(index);
                var content = HttpParser.GetBody(request);

                await Endpoints.File.Create(name, content);
                
                return createPacket;
            }
            
            return Array.Empty<byte>();
        }

        static byte[] RouteMessages(string request)
        {
            var content = HttpParser.GetBody(request);

            string userName = Endpoints.Chat.DisplayContent(content).userName;
            string message = Endpoints.Chat.DisplayContent(content).message;
                
            var responseStatusCode = HttpProtocol.StatusLine.Ok;
            var responseBody = Encoding.UTF8.GetBytes($"{userName}: {message}");

            var responseHeaders = new List<byte[]> 
            { 
                HttpProtocol.HttpHeader.Date, 
                HttpProtocol.HttpHeader.ContentLength(responseBody.Length), 
                HttpProtocol.HttpHeader.ContentTypeTextStream, 
                HttpProtocol.HttpHeader.ConnectionKeepAlive 
            };
                
            var responsePacket = HttpProtocol.Builder.BuildResponse(responseStatusCode, responseHeaders, responseBody);
                
            return responsePacket;
        }

        static async Task<string> ProcessMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            int read;
            
            read = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            
            Decoder decoder = Encoding.UTF8.GetDecoder(); 
            char[] chars = new char[decoder.GetCharCount(buffer, 0, read)]; 
            decoder.GetChars(buffer, 0, read, chars, 0); 
            builder.Append(chars);

            return builder.ToString();
        }

        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }

        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
            Console.WriteLine("Is mutually authenticated: {0}", stream.IsMutuallyAuthenticated);
        }

        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }

            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("\nto start the server enter the path to your cert.pfx in the main function and enter your password when prompted\n");
            Console.WriteLine("Password: \n");
        }

        private class Endpoints
        {
            public static Task<byte[]> IndexHtml()
            {
                var htmlStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/index.html", FileMode.Open, FileAccess.Read);

                try
                {
                    byte[] buffer = new byte[htmlStream.Length];
                    htmlStream.ReadAsync(buffer, 0, buffer.Length).Wait();
                    
                    return Task.FromResult(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                
            }

            public static Task<byte[]> IndexJs()
            {
                var jsStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/main.js", FileMode.Open, FileAccess.Read);

                try
                {
                    byte[] buffer = new byte[jsStream.Length];
                    jsStream.ReadAsync(buffer, 0, buffer.Length).Wait();

                    return Task.FromResult(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            
            public static Task<byte[]> IndexCss()
            {
                var cssStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/style.css", FileMode.Open, FileAccess.Read);

                try
                {
                    byte[] buffer = new byte[cssStream.Length];
                    cssStream.ReadAsync(buffer, 0, buffer.Length).Wait();

                    return Task.FromResult(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
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

                public static void GetContent(string fileName)
                {
                    FileStream readStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.Open, FileAccess.Read);
                }

                public static void Compress(string fileName)
                {
                    FileStream fileStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
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
                    var cssStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.css", FileMode.Open, FileAccess.Read);

                    try
                    {
                        byte[] buffer = new byte[cssStream.Length];
                        cssStream.ReadAsync(buffer, 0, buffer.Length).Wait();
                    
                        return Task.FromResult(buffer);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;
                    }
                
                }
                
                public static Task<byte[]> ChatJs()
                {
                    var jsStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.js", FileMode.Open, FileAccess.Read);

                    try
                    {
                        byte[] buffer = new byte[jsStream.Length];
                        jsStream.ReadAsync(buffer, 0, buffer.Length).Wait();
                    
                        return Task.FromResult(buffer);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;
                    }
                
                }
                
                public static Task<byte[]> ChatHtml()
                {
                    var htmlStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/Chat/chat.html", FileMode.Open, FileAccess.Read);

                    try
                    {
                        byte[] buffer = new byte[htmlStream.Length];
                        htmlStream.ReadAsync(buffer, 0, buffer.Length).Wait();
                    
                        return Task.FromResult(buffer);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;
                    }
                
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
}
