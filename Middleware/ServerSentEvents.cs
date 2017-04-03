using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PoorMan.SSE.Extensions;
using PoorMan.SSE.Impl;
using PoorMan.SSE.Interface;
using PoorMan.SSE.Models;

namespace PoorMan.SSE.Middleware {

    public static class ServerSentEvents {

        public static IServiceCollection AddServerSentEvents(this IServiceCollection services) {
            return services
                .AddSingleton<EventDispatcher>()
                .AddSingleton<IEventDispatcher, EventDispatcher>();
        }

        public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder builder) {
            return builder.UseMiddleware<Middleware>();
        }

        private class Middleware {

            private readonly RequestDelegate _next;

            private readonly EventDispatcher _dispatcher;

            public Middleware(RequestDelegate next, EventDispatcher dispatcher) {
                _next = next;
                _dispatcher = dispatcher;
            }

            public async Task Invoke(HttpContext context) {

                // This is our endpoint, should be configurable
                if ("/sse" == context.Request.Path.ToString() && "GET" == context.Request.Method) {

                    // This is sse so set the correct content type
                    context.Response.Headers.Add("Content-Type", "text/event-stream");

                    // Create the client
                    var client = new Client(
                        Guid.NewGuid().ToString()
                    );

                    // Subscribe for events
                    _dispatcher.Subscribe(client);

                    // Keep streaming out events until client disconnects
                    try { while (!context.RequestAborted.IsCancellationRequested) {
                        // Wait for an event
                        var evt = client.Queue.Take(context.RequestAborted);

                        // Write it out
                        await context.Response.WriteAsync(evt, context.RequestAborted);
                    }

                    } catch (OperationCanceledException) {
                    // We expect this exception to occur so lets just supress it

                    // Always unsubscribe
                    } finally {
                        _dispatcher.Unsubscribe(client);
                    }

                // This is our endpoint, should be configurable
                } else if ("/sse" == context.Request.Path.ToString() && "POST" == context.Request.Method) {

                    foreach(var client in _dispatcher.Clients) {
                        client.SendEvent("ping/pong");
                    }

                // Nothing to do here, move along.
                } else {
                    await _next(context);
                }
            }
        }
    }
}
