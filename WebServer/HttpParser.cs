using System.Reflection.Metadata.Ecma335;

namespace WebServer
{
    public class HttpParser
    {
        public static string GetMethod(string request)
        {
            string method;

            try
            {
                method = request.Split('/')[0];
                method = method.Replace(" ", "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return method;
        }
        
        public static string GetDomain(string request)
        {
            string url;

            try
            {
                var lines = request.Split("\r\n");
                var requestLine = lines[0].Split(" ");
                url = requestLine[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
            return url;
        }

        
        public static string GetVersion(string request, string url, string method)
        {
            string version;

            try
            {
                var prefix = method.Length + url.Length + 2;
                var line = request.Substring(prefix);
                version = line.Split("\r\n")[0];
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing HTTP Version");
                throw;
            }
            
            return version;
        }
        
        public static (string headers, string userAgent, string wsKey) GetHeader(string request)
        {
            string headers;
            string userAgent;
            string wsKey;
            
            try
            {
                var line = request.Split("\r\n");
                int toTrim = line[0].Length;
                
                headers = request.Substring(toTrim);

                if (headers.Contains("Sec-WebSocket-Key"))
                {
                    wsKey = headers.Split("Sec-WebSocket-Key")[1];
                    wsKey = wsKey.Split("\r\n")[0];
                    wsKey = wsKey.Split(" ")[1];
                }
                else
                {
                    wsKey = String.Empty;
                }
                
                if (headers.Contains("User-Agent"))
                {
                    userAgent = headers.Split("User-Agent")[1];
                    userAgent = userAgent.Split("\r\n")[0];
                }
                else
                {
                    userAgent = String.Empty;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return (headers, userAgent, wsKey);
        }

        public static string GetBody(string request)
        {
            string body;

            try
            {
                body = request.Split("\r\n\r\n")[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
            return body;
        }
    }
}

