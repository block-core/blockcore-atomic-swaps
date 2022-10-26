using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using NBitcoin;
using NBitcoin.Policy;
using Xunit;

namespace Blockcore.AtomicSwaps.Test
{
    public class AtomicSwapTest
    {
        [Fact]
        public void PerformAnAtomicSwap()
        {

            // in this example we swap coins from strax to city
            // the seller is on the city chain and offers to sell 10 city for 1 strax


            string senderKeySTRAXString = "";
            string receiverKeySTRAXString = "";
            string senderKeyCITYString = "";
            string receiverKeyCITYString = "";
            string sharedSecretString;

            Key senderKeySTRAX = new Key();
            Key receiverKeySTRAX = new Key();
            Key senderKeyCITY = new Key();
            Key receiverKeyCITY = new Key();
            uint256 sharedSecret;

            // Phase 1: seller bails in

            // seller creates the shared secret (but it must stay private)
            sharedSecret = new uint256(RandomUtils.GetBytes(32));
            var sharedSecretHash160 = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes());

            // seller builds the trx that sends 10 city to the buyer

            var cityNetwork = Networks.Networks.City.Mainnet();

            // create a fake inputTrx
            var cityFakeInputTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            Key fakeInputKey = new Key();
            var fakeTxout = cityFakeInputTrx.AddOutput(Money.Parse("20.2"), fakeInputKey.ScriptPubKey);

            // var citySwapTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();

            var citySwapTrx = SwapBuilder.CreateSwapTransaction(
                cityNetwork,
                sharedSecretHash160,
                senderKeyCITY.PubKey,
                senderKeyCITY.PubKey,
                receiverKeyCITY.PubKey, 
                DateTime.UtcNow.AddHours(48),
                Money.Parse("10.1"),
                new List<Utxo>() { new Utxo { OutPoint = new OutPoint(cityFakeInputTrx, 0), Amount = fakeTxout.Value, PrivateKey = fakeInputKey, Script = fakeTxout.ScriptPubKey } },
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 2)));

            // option 1 receiver can now claim the swap

            var swapSpendTransaction = SwapBuilder.CreateSwapSpendTransaction(
                cityNetwork,
                citySwapTrx,
                sharedSecret,
                receiverKeyCITY.PubKey,
                receiverKeyCITY,
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee)));

            // option 2 sender can recover the trx after enough time has passed

            var swapRecoverTransaction = SwapBuilder.CreateSwapRecoveryTransaction(
                cityNetwork,
                citySwapTrx,
                senderKeyCITY.PubKey,
                senderKeyCITY,
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee)),
                DateTime.UtcNow.AddHours(48));

            // 1 - create an htlc trx that sends from seller to buyer

            // 2 - ask buyer to sign their side

            // Phase 2: buyer bails in



            // Phase 3: seller reveals S and triggers both transactions
        }
    }
}