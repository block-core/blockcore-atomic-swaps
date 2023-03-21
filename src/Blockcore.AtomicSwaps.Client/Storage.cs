﻿using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.Shared;

namespace Blockcore.AtomicSwaps.Client
{
    public class Storage
    {
        private readonly ISyncLocalStorageService _storage;

        public Storage(ISyncLocalStorageService storage)
        {
            _storage = storage;
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

        public string? GetExplorerUrl()
        {
            return _storage.GetItemAsString("explorer");
        }

        public void SetIndexerUrl(string symbol, string url)
        {
            _storage.SetItemAsString(symbol.ToLower() + "-indexer", url);
        }

        public string? GetIndexerUrl(string symbol)
        {
            return _storage.GetItemAsString(symbol + "-indexer");
        }
    }
}
