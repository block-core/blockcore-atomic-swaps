using Blockcore.Features.Wallet.Helpers;
using NBitcoin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Blockcore.AtomicSwaps.Test
{
    public class UtilTest
    {
        [Fact]
        public void TestKeys()
        {
            ExtKey.UseBCForHMACSHA512 = true;
            NBitcoin.Crypto.Hashes.UseBCForHMACSHA512 = true;

            Networks.Network network = Blockcore.Networks.Networks.Strax.Mainnet();

            var coinType = network.Consensus.CoinType;
            var accountIndex = 0; // for now only account 0
            var purpose = 44; // for now only legacy

            var walletWrods = "silver napkin mind program secret junk bottom pair check skate crunch olive";

            ExtKey extendedKey;
            try
            {
                extendedKey = HdOperations.GetExtendedKey(walletWrods);

            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Exception occurred: {0}", ex.ToString());

                if (ex.Message == "Unknown")
                    throw new Exception("Please make sure you enter valid mnemonic words.");

                throw;
            }

            string accountHdPath = HdOperations.GetAccountHdPath(purpose, coinType, accountIndex);
            Key privateKey = extendedKey.PrivateKey;
            ExtPubKey accountExtPubKeyTostore = HdOperations.GetExtendedPublicKey(privateKey, extendedKey.ChainCode, accountHdPath);

            var extPubKey = accountExtPubKeyTostore.ToString(network);


            ExtPubKey accountExtPubKey = ExtPubKey.Parse(extPubKey, network);

            List<(string, string, string)> lst = new();

            var gap = 5;
            for (int scanIndex = 0; scanIndex < 20; scanIndex++)
            {
                PubKey pubkey = HdOperations.GeneratePublicKey(accountExtPubKey, scanIndex, false);
                var adddress = pubkey.GetAddress(network).ToString();
                var path = HdOperations.CreateHdPath(purpose, coinType, accountIndex, false, scanIndex);

                lst.Add((adddress, path, pubkey.ToHex(network.Consensus.ConsensusFactory)));
            }
        }
    }
}