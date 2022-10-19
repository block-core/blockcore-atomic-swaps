using System;
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


            // seller builds the trx that sends 10 city to the buyer

            // create a fake inputTrx
            var cityFakeInputTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            Key fakeInputKey = new Key();
            cityFakeInputTrx.AddOutput(Money.Parse("20.2"), fakeInputKey.ScriptPubKey);

            var citySwapTrx = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();

            // add some fake inputs, we assume it is an input with 20 city
            citySwapTrx.AddInput(cityFakeInputTrx, 0);


            // add the change output (change will go to the same address as the swap)
            citySwapTrx.AddOutput(Money.Coins(10), senderKeyCITY.ScriptPubKey);

            // the lock time n the sellet that created the shred secret is 48 hours
            var locktime = Utils.DateTimeToUnixTime(DateTime.UtcNow.AddHours(48));

            // make the swap script
            Script swapScript = SwapScripts.GetAtomicSwapHtlcScript(locktime, senderKeyCITY.PubKey, receiverKeyCITY.PubKey, sharedSecret);

            // we of course use p2sh
            //Script swapScriptHash = swapScript.GetScriptAddress(Networks.Networks.City.Mainnet()).ScriptPubKey;

            // now we need to add the swap trx
            citySwapTrx.AddOutput(Money.Parse("10.1"), swapScript);


            TransactionBuilder builder = new TransactionBuilder(Networks.Networks.City.Mainnet());
            builder.AddKeys(fakeInputKey);
            builder.AddCoins(cityFakeInputTrx);
            var signedCitySwapTrx = builder.SignTransaction(citySwapTrx);

            var verifyresult = builder.Verify(signedCitySwapTrx, out TransactionPolicyError[] result);
            Assert.Equal("Non-Standard scriptPubKey", result[0].ToString());

            // broadcast this trx signedCitySwapTrx


            // option 1 receiver can now claim the swap

            var claimTransaction = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            TxIn swapInput = claimTransaction.AddInput(signedCitySwapTrx, 1);
            claimTransaction.AddOutput(Money.Coins(10), receiverKeyCITY.ScriptPubKey);

            uint256 sighash = claimTransaction.GetSignatureHash(Networks.Networks.City.Mainnet(), new Coin(signedCitySwapTrx, 1));
            TransactionSignature signature = receiverKeyCITY.Sign(sighash, SigHash.All);

            swapInput.ScriptSig = SwapScripts.GetAtomicSwapExchangeScriptSig(signature, sharedSecret);

            TransactionBuilder builder1 = new TransactionBuilder(Networks.Networks.City.Mainnet());
            builder1.AddCoins(signedCitySwapTrx);
            verifyresult = builder1.Verify(claimTransaction, out TransactionPolicyError[] result1);
            Assert.True(verifyresult);

            // option 2 sender can recover the trx after enough time has passed
            var recoverTransaction = Networks.Networks.City.Mainnet().Consensus.ConsensusFactory.CreateTransaction();
            TxIn swapInput1 = recoverTransaction.AddInput(signedCitySwapTrx, 1);
            recoverTransaction.AddOutput(Money.Coins(10), senderKeyCITY.ScriptPubKey);

            var locktime1 = Utils.DateTimeToUnixTime(DateTime.UtcNow.AddHours(58));
            recoverTransaction.LockTime = locktime1;
            swapInput1.Sequence = new Sequence(locktime1);

            uint256 sighash1 = recoverTransaction.GetSignatureHash(Networks.Networks.City.Mainnet(), new Coin(signedCitySwapTrx, 1));
            TransactionSignature signature1 = senderKeyCITY.Sign(sighash1, SigHash.All);

            swapInput1.ScriptSig = SwapScripts.GetAtomicSwapRecoverScriptSig(signature1);

            TransactionBuilder builder2 = new TransactionBuilder(Networks.Networks.City.Mainnet());
            builder2.AddCoins(signedCitySwapTrx);
            verifyresult = builder2.Verify(recoverTransaction, out TransactionPolicyError[] result2);
            Assert.True(verifyresult);


            // 1 - create an htlc trx that sends from seller to buyer

            // 2 - ask buyer to sign their side

            // Phase 2: buyer bails in



            // Phase 3: seller reveals S and triggers both transactions
        }
    }
}