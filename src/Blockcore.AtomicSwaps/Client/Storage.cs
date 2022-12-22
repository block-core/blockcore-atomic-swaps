using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.Server.Controllers;

namespace Blockcore.AtomicSwaps.Client
{
    public class Storage
    {
        private readonly ISyncLocalStorageService _storage;

        public Storage(ISyncLocalStorageService storage)
        {
            _storage = storage;
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

        //public AccountInfo? GetAccountInfo(string network)
        //{
        //    return _storage.GetItem<AccountInfo>($"utxo:{network}");
        //}

        //public void SetAccountInfo(string network, AccountInfo items)
        //{
        //    _storage.SetItem($"utxo:{network}", items);
        //}

        public void SetSwaps(List<SwapSession> swapSessions)
        {
            _storage.SetItem($"swaps", swapSessions);
        }

        public List<SwapSession>? GetSwaps()
        {
            return _storage.GetItem<List<SwapSession>>($"swaps");
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
    }
}
