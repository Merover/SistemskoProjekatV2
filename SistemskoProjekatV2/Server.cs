using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace SysProjekat
{
    internal class AsyncServer
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory());
        static int index = 0;
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task StartWebServer()
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Accepting requests...");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting request: {ex.Message}");
                }
            }
        }

        static async Task HandleRequestAsync(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
                return;
            }

            string requestUrl = request.Url.LocalPath;
            byte[]? cachedFile = cache.Get(requestUrl) as byte[];
 

            if (cachedFile != null)
            {
                ServeCachedFile(response, requestUrl, cachedFile);
            }
            else
            {
                await ServeFileFromDiskAsync(response, requestUrl);
            }
        }

        static void ServeCachedFile(HttpListenerResponse response, string requestUrl, byte[] cachedFile)
        {
            response.ContentType = "image/gif";
            response.ContentLength64 = cachedFile.Length;
            response.OutputStream.Write(cachedFile, 0, cachedFile.Length);
            Console.WriteLine($"Cached content found for request: {requestUrl}");
            response.Close();
        }

        static async Task ServeFileFromDiskAsync(HttpListenerResponse response, string requestUrl)
        {
            Console.WriteLine($"Received the following request: {requestUrl}");

            Stopwatch stopwatch = Stopwatch.StartNew();

            string filePath = Path.Combine(RootFolder, requestUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
               
                string gifpath = await ConvertClass.ConvertToGifAsync(filePath, ++index);

                byte[] gifBytes = File.ReadAllBytes(gifpath);
                cache.Set(requestUrl, gifBytes, DateTimeOffset.Now.AddMinutes(10));

                response.ContentType = "image/gif";
                response.ContentLength64 = gifBytes.Length;
                await response.OutputStream.WriteAsync(gifBytes, 0, gifBytes.Length);

                Console.WriteLine("Created a .gif file.");
            }
            else
            {
                RespondWithNotFound(response, requestUrl);
            }

            response.Close();
            stopwatch.Stop();
            Console.WriteLine($"Processed the request in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }

        static void RespondWithNotFound(HttpListenerResponse response, string requestUrl)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            string errorMessage = $"File not found: {requestUrl}";
            byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            response.Close();
        }
    }
}
