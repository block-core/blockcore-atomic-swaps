using Blockcore.AtomicSwaps.Server.Controllers;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.Features.Wallet.Helpers;
using Blockcore.Networks;
using Blockcore.Utilities;
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

        public static uint256 GenerateSecret(Networks.Network network, Storage storage, string sessionsId)
        {
            var extendedKey = ExtKey.Parse(storage.GetWalletPrivkey(), network);
            var privateBytes = extendedKey.PrivateKey.ToBytes().ToList();
            var sessionBytes = System.Text.Encoding.UTF8.GetBytes(sessionsId);
            privateBytes.AddRange(sessionBytes);
            var secret = Hashes.Hash256(privateBytes.ToArray());
            return secret;
        }

        public static PubKey GetNextAvailableAddress(Networks.Network network, Storage storage, bool isChange = false)
        {
            AccountInfo? accountInfo = storage.GetAccountInfo(network.CoinTicker);
            Guard.NotNull(accountInfo, nameof(accountInfo));
            ExtPubKey accountExtPubKey = ExtPubKey.Parse(accountInfo.ExtPubKey, network);
            var index = isChange ? accountInfo.LastFetchChangeIndex : accountInfo.LastFetchIndex;
            if (network.IsSingleAccountNetwork())
                index = 0;
            PubKey pubKey = HdOperations.GeneratePublicKey(accountExtPubKey, index, isChange);
            return pubKey;
        }

        public static bool FindInputs(Networks.Network network, Storage storage, long targetAmount, long fee, out List<UtxoData> balancesList)
        {
            AccountInfo? accountInfo = storage.GetAccountInfo(network.CoinTicker);
            Guard.NotNull(accountInfo, nameof(accountInfo));

            var allItems = accountInfo.UtxoChangeItems.Values.SelectMany(s => s).ToList();
            allItems.AddRange(accountInfo.UtxoItems.Values.SelectMany(s => s).ToList());

            balancesList = new();
            long balance = 0;
            long total = fee + targetAmount;

            foreach (var item in allItems)
            {
                balancesList.Add(item);
                balance += item.value;

                if (balance >= total)
                    break;
            }

            if (balance < total)
                return false;

            return true;
        }

        public static List<Utxo> AddPrivateKeys(Networks.Network network, Storage storage, List<UtxoData> balancesList)
        {
            var extendedKey = ExtKey.Parse(storage.GetWalletPrivkey(), network);

            var utxos = balancesList.Select(s => new Utxo
            {
                Amount = s.value,
                OutPoint = new OutPoint(new uint256(s.outpoint.transactionId), s.outpoint.outputIndex),
                PrivateKey = HdOperations.GetExtendedPrivateKey(extendedKey.PrivateKey, extendedKey.ChainCode, s.HdPath, network).PrivateKey,
                Script = Script.FromHex(s.scriptHex)
            }).ToList();

            return utxos;
        }
    }

    public class IndexerUrl
    {
        public string Symbol { get; set; }
        public string Url { get; set; }
    }
}
