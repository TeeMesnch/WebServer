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
                
                var statusCode = HttpProtocol.StatusLine.Ok;
                
                var body = Encoding.UTF8.GetBytes("Hello Client!");
                
                var headers = new List<byte[]>
                {
                    HttpProtocol.HttpHeader.ContentLength(body.Length),
                    HttpProtocol.HttpHeader.Date,
                    HttpProtocol.HttpHeader.ConnectionKeepAlive,
                };
                
                var packet = HttpProtocol.Builder.BuildResponse(statusCode, headers, body);
                
                await sslStream.WriteAsync(packet, 0, packet.Length);
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
    }
}
