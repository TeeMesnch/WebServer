using System.Text;

namespace WebServer
{
    class Routes
    {
        public static async Task<byte[]> RouteIndexHtml(string request)
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

        public static async Task<byte[]> RouteIndexJs(string request)
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
        
        public static async Task<byte[]> RouteIndexCss(string request)
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

        public static async Task<byte[]> RouteEcho(string request)
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

        public static async Task<byte[]> RouteNotFound(string request)
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

        public static byte[] RouteTimeout(string request)
        {
            var timeoutStatusCode = HttpProtocol.StatusLine.RequestTimeout;
            var timeoutBody = Array.Empty<byte>();

            var timeoutHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.RetryAfter,
                HttpProtocol.HttpHeader.ContentLength(0),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var timeoutPacket = HttpProtocol.Builder.BuildResponse(timeoutStatusCode, timeoutHeaders, timeoutBody);
            
            return timeoutPacket;
        }

        public static async Task<byte[]> RouteChatHtml(string request)
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

        public static async Task<byte[]> RouteChatJs(string request)
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

        public static async Task<byte[]> RouteChatCss(string request)
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

                await Endpoints.File.Create(name, content);
                
                return createPacket;
            }
            
            return Array.Empty<byte>();
        }

        public static async Task<byte[]> RouteCompressFile(string request)
        {
            string fileName = HttpParser.GetDomain(request).Substring("/file/compress/".Length);

            try
            {
                Endpoints.File.Compress(fileName);
            }
            catch (Exception)
            {
                Console.WriteLine("Error compressing file: " + fileName);
            }
            
            var compressStatusCode = HttpProtocol.StatusLine.Created;
            var compressBody = Array.Empty<byte>();

            var compressHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentLength(0),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var compressPacket = HttpProtocol.Builder.BuildResponse(compressStatusCode, compressHeaders, compressBody);

            return compressPacket;
        }

        public static async Task<byte[]> RouteMessages(string request)
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

        public static async Task<byte[]> RouteVideoHtml(string request)
        {
            var videoHtmlCode = HttpProtocol.StatusLine.Ok;
            var videoHtmlBody = await Endpoints.VideoHtml();

            var videoCssHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeHtml,
                HttpProtocol.HttpHeader.ContentLength(videoHtmlBody.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
           
            var videoCssPacket = HttpProtocol.Builder.BuildResponse(videoHtmlCode, videoCssHeaders, videoHtmlBody);
           
            return videoCssPacket;
        }

        public static async Task<byte[]> RouteVideoCss(string request)
        {
           var videoCssStatusCode = HttpProtocol.StatusLine.Ok; 
           var videoCssBody = await Endpoints.VideoCss();

           var videoCssHeaders = new List<byte[]>
           {
               HttpProtocol.HttpHeader.Date,
               HttpProtocol.HttpHeader.ContentTypeCss,
               HttpProtocol.HttpHeader.ContentLength(videoCssBody.Length),
               HttpProtocol.HttpHeader.ConnectionClose
           };
           
           var videoCssPacket = HttpProtocol.Builder.BuildResponse(videoCssStatusCode, videoCssHeaders, videoCssBody);
           
           return videoCssPacket;
        }
        
        public static async Task<byte[]> RouteVideoMp4(string request)
        {
            var videoMp4StatusCode = HttpProtocol.StatusLine.Ok;
            var videoBytes = Endpoints.VideoMp4().Result;

            var videoHeaders = new List<byte[]>
            {
                HttpProtocol.HttpHeader.Date,
                HttpProtocol.HttpHeader.ContentTypeVideoMp4,
                HttpProtocol.HttpHeader.ContentLength(videoBytes.Length),
                HttpProtocol.HttpHeader.ConnectionClose
            };
            
            var videoPacket = HttpProtocol.Builder.BuildResponse(videoMp4StatusCode, videoHeaders, videoBytes);

            return videoPacket;
        }
    }
}