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
        
        public static (string headers, string userAgent) GetHeader(string request)
        {
            string headers;
            string userAgent;
            
            try
            {
                var line = request.Split("\r\n");
                int toTrim = line[0].Length;
                
                headers = request.Substring(toTrim);

                if (headers.Contains("User-Agent"))
                {
                    userAgent = headers.Split("User-Agent")[1];
                    userAgent = userAgent.Split("\r\n")[0];
                }
                else
                {
                    throw new Exception("No user agent found");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return (headers, userAgent);
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

