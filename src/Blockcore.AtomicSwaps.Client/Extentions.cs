using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Blockcore.AtomicSwaps.Client
{
    public static class Extensions
    {
        public static bool IsSingleAccountNetwork(this Networks.Network network)
        {
            if (network.CoinTicker == "CRS") return true;

            return false;
        }

        public static void ThorwIfError(this string error)
        {
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
        }

        public static async Task<TValue?> GetFromJsonNullableAsync<TValue>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken = default)
        {
            string? res = await client.GetStringAsync(requestUri, cancellationToken);

            if (string.IsNullOrEmpty(res))
            {
                return default(TValue);
            }

            var ser = JsonSerializer.Deserialize<TValue>(res, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return ser;
        }
    }
}
