using System.Text;

namespace Parser
{
    class HttpParser
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
        
        public static async Task<string> GetHeader(string request)
        {
            return string.Empty;
        }

        public static async Task<string> GetBody(string request)
        {
            return string.Empty;
        }
    }
}

