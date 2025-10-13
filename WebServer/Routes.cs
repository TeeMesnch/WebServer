using System.Text;

namespace WebServer
{
    class Routes
    {
        public static async Task<byte[]> RouteIndexHtml()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;
            
            var indexHtmlBody = await Server.Endpoints.IndexHtml();
                
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

        public static async Task<byte[]> RouteIndexJs()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;

            var indexJsBody = await Server.Endpoints.IndexJs();

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
        
        public static async Task<byte[]> RouteIndexCss()
        {
            var statusCode = HttpProtocol.StatusLine.Ok;
            
            var indexCssBody = await Server.Endpoints.IndexCss();

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

        public static byte[] RouteEcho(string request)
        {
            var echoStatusCode = HttpProtocol.StatusLine.Ok;
            var echoBody = Server.Endpoints.Echo(request);

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

        public static byte[] RouteNotFound()
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

        public static async Task<byte[]> RouteChatHtml()
        {
            var chatHtmlStatusCode = HttpProtocol.StatusLine.Ok;
            var chatHtmlBody = await Server.Endpoints.Chat.ChatHtml();

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

        public static async Task<byte[]> RouteChatJs()
        {
            var chatJsStatusCode = HttpProtocol.StatusLine.Ok;
            var chatJsBody = await Server.Endpoints.Chat.ChatJs();

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

        public static async Task<byte[]> RouteChatCss()
        {
            var chatCssStatusCode = HttpProtocol.StatusLine.Ok;
            var chatCssBody = await Server.Endpoints.Chat.ChatCss();

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
        
        public static async Task<byte[]> RouteCreateFile(string request)
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

                await Server.Endpoints.File.Create(name, content);
                
                return createPacket;
            }
            
            return Array.Empty<byte>();
        }

        public static byte[] RouteMessages(string request)
        {
            var content = HttpParser.GetBody(request);

            string userName = Server.Endpoints.Chat.DisplayContent(content).userName;
            string message = Server.Endpoints.Chat.DisplayContent(content).message;
                
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
    }
}