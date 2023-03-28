using System.Net.Http.Json;
using Blockcore.AtomicSwaps.Shared;

namespace Blockcore.AtomicSwaps.Client.Services
{

    public class BlockchainApiService : IBlockchainApiService
    {
        private readonly ILogger<BlockchainApiService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SwapsConfiguration _swapsConfiguration;
        private readonly Storage _storage;

        public BlockchainApiService(ILogger<BlockchainApiService> logger, HttpClient httpClient, SwapsConfiguration swapsConfiguration, Storage storage)
        {
            _logger = logger;
            _httpClient = httpClient;
            _swapsConfiguration = swapsConfiguration;
            _storage = storage;
        }

        public async Task<int> GetConfirmationsAsync(string network, string trxId, string? trxHex = null)
        {
            try
            {
                var indexer = (await _storage.Indexers()).First(f => f.Symbol == network);
                var url = $"/query/transaction/{trxId}";
                var res = await _httpClient.GetFromJsonNullableAsync<TransactionData>(indexer.Url + url);
                return res?.confirmations ?? 0;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, $"Failed read trx {network} {trxId}");

                // try to rebroadcast
                if (!string.IsNullOrEmpty(trxHex))
                {
                    await Broadcast(network, trxHex);
                }
            }

            return 0;
        }

        public async Task Broadcast(string network, string? trxHex)
        {
            if (!string.IsNullOrEmpty(trxHex))
            {
                var indexer = (await _storage.Indexers()).First(f => f.Symbol == network);
                var url = $"/command/send";
                var result = await _httpClient.PostAsync(indexer.Url + url, new StringContent(trxHex));
                result.EnsureSuccessStatusCode();
            }
        }
    }
}
