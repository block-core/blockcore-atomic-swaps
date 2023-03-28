using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.BlockcoreDns;
using Blockcore.AtomicSwaps.BlockcoreDns.Models;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Networks;

namespace Blockcore.AtomicSwaps.Client
{
    public class Storage
    {
        private readonly ISyncLocalStorageService _storage;
        private readonly IBlockcoreDnsService _dnsService;
        private readonly SwapsConfiguration _swapsConfiguration;

        public Storage(ISyncLocalStorageService storage, IBlockcoreDnsService dnsService, SwapsConfiguration swapsConfiguration)
        {
            _storage = storage;
            _dnsService = dnsService;
            _swapsConfiguration = swapsConfiguration;
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
            _storage.SetItemAsString("explorer", "https://" + address);
        }

        public string? GetExplorerUrl()
        {
            var res = _storage.GetItemAsString("explorer");

            return res;
        }

        public void SetIndexerUrl(string symbol, string url)
        {
            _storage.SetItemAsString(symbol.ToLower() + "-indexer", "https://" + url + "/api");
        }

        public string? GetIndexerUrl(string symbol)
        {
            var res = _storage.GetItemAsString(symbol.ToLower() + "-indexer");

            return res;
        }

        public async Task FetchIndexerAndExplorer(bool forceRefresh)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_storage.GetItemAsString("explorer")))
                return;

            var indexers = await _dnsService.GetServicesByType("Indexer");

            var networks = _swapsConfiguration.Networks.Keys.ToList();

            foreach (var index in indexers.ToList())
            {
                foreach (var onlineIndexer in index.DnsResults.Where(w => w.Online).ToList())
                {
                    if (networks.Contains(onlineIndexer.Symbol))
                    {
                        if (onlineIndexer != null)
                        {
                            if (string.IsNullOrEmpty(GetIndexerUrl(onlineIndexer.Symbol)))
                            {
                                SetIndexerUrl(onlineIndexer.Symbol, onlineIndexer.Domain);
                            }
                        }
                    }
                }
            }

            var explorers = await _dnsService.GetServicesByType("Explorer");
            foreach (var index in explorers)
            {
                var onlineExplorers = index.DnsResults.FirstOrDefault(c => c.Online);
                if (onlineExplorers != null)
                {
                    SetExplorerUrl(onlineExplorers.Domain);
                }
            }
        }

        public async Task<string?> ExplorerUrl()
        {
            await FetchIndexerAndExplorer(false);

            var res = GetExplorerUrl();

            if (string.IsNullOrEmpty(res))
            {
                throw new Exception($"Explorer link was not found please go to settings page to lookup for indexer urls using a DNS.");
            }

            return res;
        }

        public async Task<IndexerUrl> Indexer(string symbol)
        {
            await FetchIndexerAndExplorer(false);

            var res = GetIndexerUrl(symbol);

            if (string.IsNullOrEmpty(res))
            {
                throw new Exception($"Indexer api link was not found for symbol '{symbol}' please go to settings page to lookup for indexer urls using a DNS.");
            }

            return new IndexerUrl { Symbol = symbol, Url = res };
        }
    }
}
