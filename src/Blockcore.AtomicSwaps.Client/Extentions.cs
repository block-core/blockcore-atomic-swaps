﻿using Blockcore.AtomicSwaps.Client.Services;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
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

        public static void ThorwIfError(this string error)
        {
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
        }
    }
}