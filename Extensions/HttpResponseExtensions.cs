using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PoorMan.SSE.Models;

namespace PoorMan.SSE.Extensions {

    internal static class HttpReponseExtensions {

        internal async static Task WriteAsync(this HttpResponse response, ServerSentEvent evt, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!string.IsNullOrEmpty(evt.Event)) {
                await response.WriteAsync($"event: {evt.Event}\r", cancellationToken);
            }

            await response.WriteAsync($"data: {evt.Data}\r\r", cancellationToken);
            await response.Body.FlushAsync();
        }
    }
}
