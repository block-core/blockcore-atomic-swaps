using Blockcore.AtomicSwaps.Client.Services;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.Networks;
using Blockcore.Utilities;
using Blockcore.NBitcoin;
using Blockcore.NBitcoin.Crypto;
using System.Reflection;
using Blockcore.NBitcoin.BIP32;

namespace Blockcore.AtomicSwaps.Client
{
    public class SwapsConfiguration
    {

        public Dictionary<string, SwapSession> Swaps { get; } = new();

        public Dictionary<string, Network> Networks { get; } = new()
        {
            { "STRAX", AtomicSwaps.Shared.Networks.Networks.Strax.Mainnet() },
            { "CITY", AtomicSwaps.Shared.Networks.Networks.City.Mainnet() },
            { "BTC", AtomicSwaps.Shared.Networks.Networks.Bitcoin.Mainnet() },
            { "IMPLX", AtomicSwaps.Shared.Networks.Networks.Implx.Mainnet() },
            { "RSC", AtomicSwaps.Shared.Networks.Networks.RSC.Mainnet() },
            { "SBC", AtomicSwaps.Shared.Networks.Networks.SBC.Mainnet() },
        };

        public static string GetVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : string.Empty;
        }

        public long GetNetworkFee(string symbol)
        {
            return Networks[symbol].MinRelayTxFee * 3;
        }

        public (long ServiceFee, string ServiceAddress) GetServiceData(string symbol)
        {
            return (0, string.Empty);
        }

        public static uint256 GenerateSecret(string payload, string sessionsId)
        {
            //var extendedKey = ExtKey.Parse(storage.GetWalletPrivkey(), network);
            //var privateBytes = extendedKey.PrivateKey.ToBytes().ToList();
            var sessionBytes = System.Text.Encoding.UTF8.GetBytes(sessionsId).ToList();
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(sessionsId).ToList();
            payloadBytes.AddRange(sessionBytes);
            var secret = Hashes.Hash256(payloadBytes.ToArray());
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

        public static string GetNextAvailableAddress1(WalletConnectInput walletConnectInputs, WalletAccount walletAccount)
        {
            Guard.NotNull(walletConnectInputs, nameof(walletConnectInputs));
            Guard.NotNull(walletConnectInputs.WalletApiMessage, nameof(walletConnectInputs.WalletApiMessage));

            var account = walletConnectInputs.WalletApiMessage.response.accounts.Single(f => f.id == walletAccount.AccountId);

            return account.state.receive.Last().address;
        }

        public static bool FindInputs1(Networks.Network network, Storage storage, long targetAmount, long fee, WalletConnectInput walletConnectInputs, out List<UtxoData> balancesList)
        {
            // AccountInfo? accountInfo = storage.GetAccountInfo(network.CoinTicker);
            Guard.NotNull(walletConnectInputs, nameof(walletConnectInputs));
            Guard.NotNull(walletConnectInputs.WalletApiMessage, nameof(walletConnectInputs.WalletApiMessage));

            var account = walletConnectInputs.WalletApiMessage.response.accounts.First(f => f.networkType == network.CoinTicker);

            var allItems = account.history.unspent;

            balancesList = new();
            long balance = 0;
            long total = fee + targetAmount;

            foreach (var item in allItems)
            {
                balancesList.Add(new UtxoData
                {
                    address = item.address,
                    value = item.balance,
                    outpoint = new Outpoint { outputIndex = item.index, transactionId = item.transactionHash },
                });
                balance += item.balance;

                if (balance >= total)
                    break;
            }

            if (balance < total)
                return false;

            return true;
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
                PrivateKey = HdOperations.GetExtendedPrivateKey(extendedKey.PrivateKey, extendedKey.ChainCode, s.hdPath, network).PrivateKey,
                Script = Script.FromHex(s.scriptHex)
            }).ToList();

            return utxos;
        }

        public static List<Utxo> ConvertToUtxo(Networks.Network network, List<UtxoData> balancesList)
        {
            var utxos = balancesList.Select(s => new Utxo
            {
                Amount = s.value,
                OutPoint = new OutPoint(new uint256(s.outpoint.transactionId), s.outpoint.outputIndex),
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
