using System.IO.Enumeration;
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
        public const int RetryAfter = 60; // Implement later
        private const bool Debug = false;

        static async Task Main(string[] args)
        {
            DisplayUsage();   
            
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
            
            serverCertificate = new X509Certificate2(
                certificatePath,
                certificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
            );

            TcpListener server = new TcpListener(ip);
            server.Start();

            Console.WriteLine($"starting server (address: {ip})\n");

            while (true)
            {
                var client = server.AcceptTcpClient();
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
                
                // <index>
                var indexStatusCode = HttpProtocol.StatusLine.Ok;
                var indexBody = await Endpoints.Index();
                
                var indexHeaders = new List<byte[]>
                {
                    HttpProtocol.HttpHeader.Date,
                    HttpProtocol.HttpHeader.ContentTypeHtml,
                    HttpProtocol.HttpHeader.ContentLength(indexBody.Length),
                    HttpProtocol.HttpHeader.ConnectionKeepAlive
                };
                // </index>
                
                // <echo>
                var echoStatusCode = HttpProtocol.StatusLine.Ok;
                var echoBody = await Endpoints.Echo(request);

                var echoHeaders = new List<byte[]>
                {
                    HttpProtocol.HttpHeader.Date,
                    HttpProtocol.HttpHeader.ContentTypeText,
                    HttpProtocol.HttpHeader.ContentLength(echoBody.Length),
                    HttpProtocol.HttpHeader.ConnectionKeepAlive
                };
                // </echo>
                
                // <notFound>
                var notFoundStatusCode = HttpProtocol.StatusLine.NotFound;
                var notFoundBody = Array.Empty<byte>();

                var notFoundHeaders = new List<byte[]>
                {
                    HttpProtocol.HttpHeader.Date,
                    HttpProtocol.HttpHeader.RetryAfter,
                    HttpProtocol.HttpHeader.ContentLength(0),
                };
                // </notFound>


                if (HttpParser.GetDomain(request) == "/")
                {
                    var indexPacket = HttpProtocol.Builder.BuildResponse(indexStatusCode, indexHeaders, indexBody);
                    
                    await sslStream.WriteAsync(indexPacket, 0, indexPacket.Length);
                }
                else if (HttpParser.GetDomain(request).StartsWith("/echo/"))
                {
                    var echoPacket = HttpProtocol.Builder.BuildResponse(echoStatusCode, echoHeaders, echoBody);

                    await sslStream.WriteAsync(echoPacket, 0, echoPacket.Length);
                }
                else
                {
                    var notFoundPacket =  HttpProtocol.Builder.BuildResponse(notFoundStatusCode, notFoundHeaders, notFoundBody);
                    
                    await sslStream.WriteAsync(notFoundPacket, 0, notFoundPacket.Length);
                }
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }

                Console.WriteLine("Authentication failed - closing the connection.");
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

        static async Task<string> ProcessMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            int read = -1;
            
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
            public static async Task<byte[]> Index()
            {
                var fStream = new FileStream("/Users/jonathan/Desktop/cSharp/WebServer/WebServer/index.html", FileMode.Open, FileAccess.Read);

                try
                {
                    byte[] buffer = new byte[fStream.Length];
                    fStream.ReadAsync(buffer, 0, buffer.Length).Wait();
                    
                    return buffer;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                
            }

            public static async Task<byte[]> Echo(string request)
            {
                try
                {
                    var text = HttpParser.GetDomain(request).Substring(6);
                    
                    return Encoding.UTF8.GetBytes(text);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            public class File
            {
                // TESTING DIRECTORY
                public static async Task Create(string fileName)
                {
                    FileStream createStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.Create, FileAccess.Write);
                }

                public static async Task GetContent(string fileName)
                {
                    FileStream readStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.Open, FileAccess.Read);
                }

                public static async Task Compress(string fileName)
                {
                    FileStream fileStream = new FileStream($"/Users/jonathan/Desktop/test/{fileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }
            }

            static async Task Video()
            {
                
            }

            static async Task Image()
            {
                
            }

            static async Task Chat()
            {
                
            }
        }
    }
}
