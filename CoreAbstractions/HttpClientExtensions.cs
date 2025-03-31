using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CoreAbstractions;

public static class HttpClientExtensions
{
    public static Task<Result<T>> GetFromJsonAsync<T>(
        this IHttpClientFactory httpFactoryClient,
        string                  callingClassName,
        string                  requestPath,
        JsonSerializerOptions   options,
        CancellationToken       token = default
    ) =>
        httpFactoryClient
            .CreateClient(callingClassName)
            .GetFromJsonAsync<Result<T>>(requestPath, options, token);
}
