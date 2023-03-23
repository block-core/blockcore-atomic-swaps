using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.BlockcoreDns;
using Blockcore.AtomicSwaps.BlockcoreDns.Models;
using Blockcore.AtomicSwaps.Shared;

namespace Blockcore.AtomicSwaps.Client
{
    public class Storage
    {
        private readonly ISyncLocalStorageService _storage;
        private readonly IBlockcoreDnsService _dnsService;
        public Storage(ISyncLocalStorageService storage, IBlockcoreDnsService dnsService)
        {
            _storage = storage;
            _dnsService = dnsService;
        }

        public Storage()
        {
        }

        public void SaveWalletWords(string mnemonic)
        {
            _storage.SetItemAsString("mnemonic", mnemonic);
        }

        public string? GetWalletWords()
        {
            return _storage.GetItemAsString("mnemonic");
        }

        public void SetWalletPubkey(string pubkey)
        {
            _storage.SetItemAsString("pubkey", pubkey);
        }

        public string? GetWalletPubkey()
        {
            return _storage.GetItemAsString("pubkey");
        }

        public void SetWalletPrivkey(string privkey)
        {
            _storage.SetItemAsString("privkey", privkey);
        }

        public string? GetWalletPrivkey()
        {
            return _storage.GetItemAsString("privkey");
        }

        public AccountInfo? GetAccountInfo(string network)
        {
            return _storage.GetItem<AccountInfo>($"utxo:{network}");
        }

        public void SetAccountInfo(string network, AccountInfo items)
        {
            _storage.SetItem($"utxo:{network}", items);
        }

        public void SetSwaps(List<SwapSession> swapSessions)
        {
            _storage.SetItem($"swaps", swapSessions);
        }

        public List<SwapSession>? GetSwaps()
        {
            return _storage.GetItem<List<SwapSession>>($"swaps");
        }

        public void DeleteSwaps()
        {
            _storage.RemoveItem($"swaps");
        }

        public void Delete<T>() where T : class
        {
            _storage.RemoveItem(typeof(T).Name);
        }

        public void Set<T>(T item) where T : class
        {
            _storage.SetItem(typeof(T).Name, item);
        }

        public T GetOrCreate<T>() where T : class, new()
        {
            var item = _storage.GetItem<T>(typeof(T).Name);

            if (item == null)
            {
                item = new();
                _storage.SetItem(typeof(T).Name, item);
            }

            return item;
        }
        public void SetExplorerUrl(string address)
        {
            _storage.SetItemAsString("explorer", address);
        }

        public async Task<string>? GetExplorerUrl()
        {
            return _storage.GetItemAsString("explorer") ?? await GetExplorerUrlFromDDNS();
        }

        public async Task<string>? GetExplorerUrlFromDDNS()
        {
            var Explorers = await _dnsService.GetServicesByType("Explorer");
            foreach (var index in Explorers.ToList())
            {
                var onlineExplorers = index.DnsResults.FirstOrDefault(c => c.Online);
                if (onlineExplorers != null)
                {
                    return onlineExplorers.Domain;
                }
            }
            return string.Empty;
        }

        public void SetIndexerUrl(string symbol, string url)
        {
            _storage.SetItemAsString(symbol.ToLower() + "-indexer", url);
        }

        public async Task<string?> GetIndexerUrlAsync(string symbol)
        {
            return _storage.GetItemAsString(symbol.ToLower() + "-indexer") ?? await GetIndexerUrlFromDDNS(symbol);
        }

        public async Task<string>? GetIndexerUrlFromDDNS(string network)
        {
            var indexers = await _dnsService.GetServicesByTypeAndNetwork("Indexer", network);
            foreach (var index in indexers.ToList())
            {
                var onlineIndexer = index.DnsResults.FirstOrDefault(c => c.Online);
                if (onlineIndexer != null)
                {
                    return onlineIndexer.Domain;
                }
            }
            return string.Empty;
        }

        public async Task<List<IndexerUrl>> Indexers()
        {
            List<IndexerUrl> indexers = new List<IndexerUrl>();
            indexers.Add(new IndexerUrl { Symbol = "BTC", Url = await GetIndexerUrlAsync("BTC") });
            indexers.Add(new IndexerUrl { Symbol = "STRAX", Url = await GetIndexerUrlAsync("STRAX") });
            indexers.Add(new IndexerUrl { Symbol = "CITY", Url = await GetIndexerUrlAsync("CITY") });
            indexers.Add(new IndexerUrl { Symbol = "IMPLX", Url = await GetIndexerUrlAsync("IMPLX") });
            indexers.Add(new IndexerUrl { Symbol = "RSC", Url = await GetIndexerUrlAsync("RSC") });
            indexers.Add(new IndexerUrl { Symbol = "SBC", Url = await GetIndexerUrlAsync("SBC") });
            return indexers;
        }
    }
}
