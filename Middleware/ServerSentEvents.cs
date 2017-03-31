using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace PoorMan.SSE.Middleware {

    public static class ServerSentEvents {

        public static IServiceCollection AddServerSentEvents(this IServiceCollection services) {
            return services;
        }

        public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder builder) {
            return builder.UseMiddleware<Middleware>();
        }

        private class Middleware {

            private readonly RequestDelegate _next;

            private readonly ConcurrentDictionary<Guid, BlockingCollection<string>> _connections = new ConcurrentDictionary<Guid, BlockingCollection<string>>();

            public Middleware(RequestDelegate next) {
                _next = next;
            }

            public async Task Invoke(HttpContext context) {

                // This is our endpoint, should be configurable
                if ("/sse" == context.Request.Path.ToString() && "GET" == context.Request.Method) {

                    // This is SSE so set the correct content type
                    context.Response.Headers.Add("Content-Type", "text/event-stream");

                    // Generate connection id
                    var id = Guid.NewGuid();

                    // Subscribe for notifications
                    var queue = new BlockingCollection<string>();
                    _connections[id] = queue;

                    // Keep streaming out notifications until client disconnects
                    try { while (!context.RequestAborted.IsCancellationRequested) {
                        // Wait for a notification
                        var message = queue.Take(context.RequestAborted);

                        // Send the notification to the client
                        await context.Response.WriteAsync($"data: {message}\r\r", context.RequestAborted);
                        await context.Response.Body.FlushAsync(context.RequestAborted);
                    }

                    // Always unsubscribe
                    } finally {
                        _connections.TryRemove(id, out queue);
                    }

                // This is our endpoint, should be configurable
                } else if ("/sse" == context.Request.Path.ToString() && "POST" == context.Request.Method) {

                    foreach(var connection in _connections.Select(item => item.Value)) {
                        connection.Add("ping/pong");
                    }

                // Nothing to do here, move along.
                } else {
                    await _next(context);
                }
            }
        }
    }
}
