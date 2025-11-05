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
        private static X509Certificate2 ServerCertificate;
        private static List<string> ClientList;
        public const int RetryAfter = 60;
        private const bool LocalHost = true;
        private const bool Debug = false;

        static async Task Main()
        {
            DisplayUsage();

            // TESTING DIRECTORY
            Environment.CurrentDirectory = "/Users/jonathan/Desktop/test";

            string certificatePath = "/Users/jonathan/Desktop/localhost.pfx";
            string? certificatePassword = Console.ReadLine();
            
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
                try
                {
                    var hostName = Dns.GetHostName();
                    IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);

                    IPAddress localIpAddress =
                        localhost.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                    if (localIpAddress == null) throw new Exception("No IPv4 address found for local host.");

                    ip = new IPEndPoint(localIpAddress, port);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }


            ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);
            
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
                await sslStream.AuthenticateAsServerAsync(ServerCertificate, clientCertificateRequired: false, enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13, checkCertificateRevocation: true);

                if (Debug)
                {
                    DisplaySecurityLevel(sslStream);
                    DisplaySecurityServices(sslStream);
                    DisplayCertificateInformation(sslStream);
                }

                var request = await ProcessMessage(sslStream);

                var endPointDictionary = new Dictionary<string, Task<byte[]>>
                {
                    { "/", Routes.RouteIndexHtml(request)},
                    { "/main.js", Routes.RouteIndexJs(request)},
                    { "/style.css", Routes.RouteIndexCss(request)},
                    { "/chat", Routes.RouteChatHtml(request)},
                    { "/chat.css", Routes.RouteChatCss(request)},
                    { "/chat.js", Routes.RouteChatJs(request)},
                    { "/messages", Routes.RouteMessages(request)},
                    {  "/echo/", Routes.RouteEcho(request) },
                    { "/file/create", Routes.RouteCreateFile(request) },
                    { "/file/compress/", Routes.RouteCompressFile(request) },
                    { "/video",  Routes.RouteVideoHtml(request) },
                    { "/video.css", Routes.RouteVideoCss(request) },
                    { "/video/GetVideoFile", Routes.RouteVideoMp4(request)},
                    { "/chat/", Routes.RouteChatHtml(request)},
                    { "/video/",  Routes.RouteVideoHtml(request) },
                };

                try
                {
                    if (endPointDictionary.TryGetValue(HttpParser.GetDomain(request), out var result))
                    {
                        var package = result.Result;

                        if (HttpParser.GetDomain(request) == "/video")
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(package));
                        }

                        await sslStream.WriteAsync(package, 0, package.Length);
                    } // ELSE IF TIMEOUT 
                    else
                    {
                        var notFoundPackage = await Routes.RouteNotFound(request);

                        await sslStream.WriteAsync(notFoundPackage, 0, notFoundPackage.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("error sending data");
                }
            }
            catch (AuthenticationException)
            {
                sslStream.Close();
                client.Close();
                
                Console.WriteLine("Authentication failed");
            }
            finally
            {
                await sslStream.DisposeAsync();
                sslStream.Close();
                client.Close();
            }
        }
        
        private static async Task<string> ProcessMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();

            var read = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            
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

            X509Certificate? localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate cannot be found");
            }

            X509Certificate? remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("no remote certificate found");
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("\nto start the server enter the path to your cert.pfx in the main function and enter your password when prompted\n");
            Console.WriteLine("Password: \n");
        }
    }
}
