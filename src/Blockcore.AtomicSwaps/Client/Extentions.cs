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
    public static class Extensions
    {
        public static bool IsSingleAccountNetwork(this Networks.Network network)
        {
            if (network.CoinTicker == "CRS") return true;

            return false;
        }
    }
}
