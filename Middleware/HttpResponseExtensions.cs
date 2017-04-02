using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoorMan.SSE.Middleware {

    internal static class HttpReponseExtensions {

        public async static Task WriteSSEAsync(this HttpResponse response, object data, CancellationToken cancellationToken = default(CancellationToken)) {
            // Serialize the data
            data = JsonConvert.SerializeObject(data, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            // Write it out in SSE format
            await response.WriteAsync($"data: {data}\r\r", cancellationToken);
            await response.Body.FlushAsync();
        }

        public async static Task WriteSSEAsync(this HttpResponse response, string evt, object data, CancellationToken cancellationToken = default(CancellationToken)) {
            // Serialize the data
            data = JsonConvert.SerializeObject(data, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            // Write it out in SSE format
            await response.WriteAsync($"event: {evt}\r", cancellationToken);
            await response.WriteAsync($"data: {data}\r\r", cancellationToken);
            await response.Body.FlushAsync();
        }
    }
}
