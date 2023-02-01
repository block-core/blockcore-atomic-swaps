using Blockcore.AtomicSwaps.Shared;
using Blockcore.Consensus.TransactionInfo;
using NBitcoin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Blockcore.AtomicSwaps.Test
{
    public class AtomicSwapTest
    {
        [Fact]
        public void PerformAnAtomicSwap()
        {
            // in this example we swap coins from strax to city
            // the taker is on the city chain and offers to sell 10 city for 1 strax

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

            // Phase 1: maker bails in

            // maker creates the shared secret (but it must stay private)
            sharedSecret = new uint256(RandomUtils.GetBytes(32));
            var sharedSecretHash160 = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes());

            // maker builds the trx that sends 10 city to the taker

            var cityNetwork = Shared.Networks.Networks.City.Mainnet();
            
            // create a fake inputTrx
            var cityFakeInputTrx = Shared.Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            Key fakeInputKey = new Key();
            var fakeTxout = cityFakeInputTrx.AddOutput(Money.Parse("20.2"), fakeInputKey.ScriptPubKey);

            // var citySwapTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();

            var swapTransaction = SwapBuilder.CreateSwapTransaction(
                cityNetwork,
                sharedSecretHash160,
                senderKeyCITY.PubKey,
                senderKeyCITY.PubKey.GetAddress(cityNetwork).ScriptPubKey,
                receiverKeyCITY.PubKey, 
                DateTime.UtcNow.AddHours(48),
                Money.Parse("10.1"),
                new List<Utxo>() { new Utxo { OutPoint = new OutPoint(cityFakeInputTrx, 0), Amount = fakeTxout.Value, PrivateKey = fakeInputKey, Script = fakeTxout.ScriptPubKey } },
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 3)),
                RedeemType.P2SH);

            // option 1 receiver can now claim the swap

            var swapSpendTransaction = SwapBuilder.CreateSwapSpendTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapTransaction.RedeemScript,
                sharedSecret,
                receiverKeyCITY.PubKey.GetAddress(cityNetwork).ScriptPubKey,
                receiverKeyCITY,
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 3)));

            var secret = SwapScripts.GetSecretFromScriptSig(swapSpendTransaction);

            Assert.Equal(secret, sharedSecret);

            // option 2 sender can recover the trx after enough time has passed

            var swapRecoverTransaction = SwapBuilder.CreateSwapRecoveryTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapTransaction.RedeemScript,
                senderKeyCITY.PubKey.GetAddress(cityNetwork).ScriptPubKey,
                senderKeyCITY,
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 3)),
                DateTime.UtcNow.AddHours(48));

            // 1 - create an htlc trx that sends from maker to taker

            // 2 - ask taker to sign their side

            // Phase 2: taker bails in



            // Phase 3: maker reveals S and triggers both transactions
        }

        [Fact]
        public void PerformAnAtomicSwapPartialSigs()
        {
            // in this example we swap coins from strax to city
            // the maker is on the city chain and offers to sell 10 city for 1 strax

            Key senderKeySTRAX = new Key();
            Key receiverKeySTRAX = new Key();
            Key senderKeyCITY = new Key();
            Key receiverKeyCITY = new Key();
            uint256 sharedSecret;

            // Phase 1: maker bails in

            // maker creates the shared secret (but it must stay private)
            sharedSecret = new uint256(RandomUtils.GetBytes(32));
            var sharedSecretHash160 = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes());

            // maker builds the trx that sends 10 city to the taker

            var cityNetwork = Shared.Networks.Networks.City.Mainnet();

            // create a fake inputTrx
            var cityFakeInputTrx = Shared.Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            Key fakeInputKey = new Key();
            var fakeTxout = cityFakeInputTrx.AddOutput(Money.Parse("20.2"), fakeInputKey.ScriptPubKey);

            // var citySwapTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();

            var swapTransaction = SwapBuilder.CreateSwapTransaction(
                cityNetwork,
                sharedSecretHash160,
                senderKeyCITY.PubKey,
                senderKeyCITY.PubKey.GetAddress(cityNetwork).ScriptPubKey,
                receiverKeyCITY.PubKey,
                DateTime.UtcNow.AddHours(48),
                Money.Parse("10.1"),
                new List<Utxo>() { new Utxo { OutPoint = new OutPoint(cityFakeInputTrx, 0), Amount = fakeTxout.Value, PrivateKey = fakeInputKey, Script = fakeTxout.ScriptPubKey } },
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 3)),
                RedeemType.P2SH);

            // option 1 receiver can now claim the swap

            var swapSpendUnsignedTransaction = SwapBuilder.CreateSwapSpendUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                receiverKeyCITY.PubKey.GetAddress(cityNetwork).ToString(),
                Money.Satoshis(cityNetwork.MinTxFee * 3));

            var swapSpendTransaction = SwapBuilder.SignSwapSpendUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapSpendUnsignedTransaction,
                swapTransaction.RedeemScript,
                sharedSecret,
                receiverKeyCITY);

            var secret = SwapScripts.GetSecretFromScriptSig(swapSpendTransaction);

            Assert.Equal(secret, sharedSecret);

            // option 2 sender can recover the trx after enough time has passed

            var swapRecoverUnsignedTransaction = SwapBuilder.CreateSwapRecoveryUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                senderKeyCITY.PubKey.GetAddress(cityNetwork).ToString(),
                Money.Satoshis(cityNetwork.MinTxFee * 3),
                DateTime.UtcNow.AddHours(48));


            var swapRecoverTransaction = SwapBuilder.SignSwapRecoveryUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapRecoverUnsignedTransaction, 
                swapTransaction.RedeemScript, 
                senderKeyCITY);

            // 1 - create an htlc trx that sends from maker to taker

            // 2 - ask taker to sign their side

            // Phase 2: taker bails in



            // Phase 3: maker reveals S and triggers both transactions
        }


        [Fact]
        public void PerformAnAtomicSwapSegWitPartialSigs()
        {
            // in this example we swap coins from strax to city
            // the maker is on the city chain and offers to sell 10 city for 1 strax

            Key senderKeySTRAX = new Key();
            Key receiverKeySTRAX = new Key();
            Key senderKeyCITY = new Key();
            Key receiverKeyCITY = new Key();
            uint256 sharedSecret;

            // Phase 1: maker bails in

            // maker creates the shared secret (but it must stay private)
            sharedSecret = new uint256(RandomUtils.GetBytes(32));
            var sharedSecretHash160 = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes());

            // maker builds the trx that sends 10 city to the taker

            var cityNetwork = Shared.Networks.Networks.City.Mainnet();

            // create a fake inputTrx
            var cityFakeInputTrx = Shared.Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            Key fakeInputKey = new Key();
            var fakeTxout = cityFakeInputTrx.AddOutput(Money.Parse("20.2"), fakeInputKey.ScriptPubKey);

            // var citySwapTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();

            var swapTransaction = SwapBuilder.CreateSwapTransaction(
                cityNetwork,
                sharedSecretHash160,
                senderKeyCITY.PubKey,
                senderKeyCITY.PubKey.GetAddress(cityNetwork).ScriptPubKey,
                receiverKeyCITY.PubKey,
                DateTime.UtcNow.AddHours(48),
                Money.Parse("10.1"),
                new List<Utxo>() { new Utxo { OutPoint = new OutPoint(cityFakeInputTrx, 0), Amount = fakeTxout.Value, PrivateKey = fakeInputKey, Script = fakeTxout.ScriptPubKey } },
                new FeeRate(Money.Satoshis(cityNetwork.MinTxFee * 3)),
                RedeemType.WitnessV0);

            // option 1 receiver can now claim the swap

            var swapSpendUnsignedTransaction = SwapBuilder.CreateSwapSpendUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                receiverKeyCITY.PubKey.GetSegwitAddress(cityNetwork).ToString(),
                Money.Satoshis(cityNetwork.MinTxFee * 3));

            var swapSpendTransaction = SwapBuilder.SignSwapSpendUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapSpendUnsignedTransaction,
                swapTransaction.RedeemScript,
                sharedSecret,
                receiverKeyCITY);

            var secret = SwapScripts.GetSecretFromScriptSig(swapSpendTransaction);

            Assert.Equal(secret, sharedSecret);

            // option 2 sender can recover the trx after enough time has passed

            var swapRecoverUnsignedTransaction = SwapBuilder.CreateSwapRecoveryUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                senderKeyCITY.PubKey.GetSegwitAddress(cityNetwork).ToString(),
                Money.Satoshis(cityNetwork.MinTxFee * 3),
                DateTime.UtcNow.AddHours(48));


            var swapRecoverTransaction = SwapBuilder.SignSwapRecoveryUnsignedTransaction(
                cityNetwork,
                swapTransaction.Transaction,
                swapRecoverUnsignedTransaction,
                swapTransaction.RedeemScript,
                senderKeyCITY);

            // 1 - create an htlc trx that sends from maker to taker

            // 2 - ask taker to sign their side

            // Phase 2: taker bails in



            // Phase 3: maker reveals S and triggers both transactions
        }

    }
}