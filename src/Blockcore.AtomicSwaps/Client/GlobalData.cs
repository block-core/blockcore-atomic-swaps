using Blockcore.AtomicSwaps.Server.Controllers;
using Blockcore.Features.Wallet.Helpers;
using Blockcore.Networks;
using NBitcoin;
using NBitcoin.Crypto;

namespace Blockcore.AtomicSwaps.Client
{
    public class GlobalData
    {
        public Dictionary<string, SwapSession> Swaps { get; } = new();


        public List<IndexerUrl> Indexers { get; } = new()
        {
            new IndexerUrl { Symbol = "STRAX", Url = "https://strax.indexer.blockcore.net/api" },
            new IndexerUrl { Symbol = "CITY", Url = "https://city.indexer.blockcore.net/api" },
        };

        public Dictionary<string, Network> Networks { get; } = new()
        {
            { "STRAX", Blockcore.Networks.Networks.Strax.Mainnet() },
            { "CITY", Blockcore.Networks.Networks.City.Mainnet() },
            { "BTC", Blockcore.Networks.Networks.Bitcoin.Mainnet() }
        };

        //public Dictionary<string, Dictionary<string, List<UtxoData>>> UtxoData { get; } = new()
        //{
        //    { "STRAX", new Dictionary<string, List<UtxoData>>() {}  },
        //    { "CITY", new Dictionary<string, List<UtxoData>>() { } },
        //    { "BTC", new Dictionary<string, List<UtxoData>>() { } },
        //};

        public static uint256 GenerateSecret(string walletWrods, string sessionsId)
        {
            var extendedKey = HdOperations.GetExtendedKey(walletWrods);
            var privateBytes = extendedKey.PrivateKey.ToBytes().ToList();
            var sessionBytes = System.Text.Encoding.UTF8.GetBytes(sessionsId);
            privateBytes.AddRange(sessionBytes);
            var secret = Hashes.Hash256(privateBytes.ToArray());
            return secret;
        }
    }

    public class IndexerUrl
    {
        public string Symbol { get; set; }
        public string Url { get; set; }
    }
}
