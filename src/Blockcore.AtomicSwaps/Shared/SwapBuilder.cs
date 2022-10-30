using System.Diagnostics.Contracts;
using System.Text;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.Networks;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.Policy;

namespace Blockcore.AtomicSwaps.Shared
{
    public class SwapBuilder
    {
        public static (Transaction Transaction, Script RedeemScript, string SwapAddress) CreateSwapTransaction(
            Network network, 
            uint160 sharedSecretHash, 
            PubKey senderPublicKey,
            Script scriptPubKeyChange,
            PubKey receiverPublicKey,
            DateTime lockTime,
            Money amount,
            List<Utxo> utxos,
            FeeRate feeRate,
            RedeemType redeemType)
        {
            var swapTrx = network.Consensus.ConsensusFactory.CreateTransaction();

            foreach (var utxo in utxos)
            {
                swapTrx.AddInput(new TxIn(utxo.OutPoint));
            }
            
            TxOut changeOutput = swapTrx.AddOutput(Money.Coins(0), scriptPubKeyChange);

            var locktime = Utils.DateTimeToUnixTime(lockTime);

            Script swapScript = SwapScripts.GetAtomicSwapHtlcScript(locktime, senderPublicKey, receiverPublicKey, sharedSecretHash);

            IDestination swapScriptHash = redeemType == RedeemType.P2SH ? swapScript.Hash : swapScript.WitHash;
            var swapAddress = swapScriptHash.ScriptPubKey.GetDestinationAddress(network).ToString();

            swapTrx.AddOutput(amount, swapScriptHash.ScriptPubKey);

            Money expectedFee = feeRate.GetFee(swapTrx, network.Consensus.Options.WitnessScaleFactor);
            Money totalInInputs = utxos.Sum(inp => inp.Amount);
            Money changeAmount = totalInInputs - amount - expectedFee;
            changeOutput.Value = changeAmount;

            TransactionBuilder builder = new TransactionBuilder(network)
                    .AddKeys(utxos.Select(s => s.PrivateKey).ToArray())
                    .AddCoins(utxos.Select(s => new Coin(s.OutPoint, new TxOut(s.Amount, s.Script))));
                
            var signTransaction = builder.SignTransaction(swapTrx);

            var verifyresult = builder.Verify(signTransaction, out TransactionPolicyError[] result);

            if (result.Any())
            {
                StringBuilder sb = new();
                foreach (var policyError in result)
                {
                    sb.AppendLine(policyError.ToString());
                }

                throw new Exception(sb.ToString());
            }

            return (signTransaction, swapScript, swapAddress);
        }

        public static Transaction CreateSwapSpendTransaction(
            Network network,
            Transaction swapTransaction,
            Script redeemScript,
            uint256 sharedSecret,
            Script sendToPublicKey,
            Key receiverPrivateKey,
            FeeRate feeRate)
        {
            var swapSpendTransaction = network.Consensus.ConsensusFactory.CreateTransaction();
            IndexedTxOut swapOutput = swapTransaction.Outputs.AsIndexedOutputs().Last(); // the swap is always the last output
            TxIn swapSpentInput = swapSpendTransaction.AddInput(swapTransaction, (int)swapOutput.N); 
            TxOut swapSpendTransactionTxOut = swapSpendTransaction.AddOutput(Money.Coins(0), sendToPublicKey);

            Money fee = feeRate.GetFee(swapTransaction, network.Consensus.Options.WitnessScaleFactor);
            Money send = swapOutput.TxOut.Value - fee;
            swapSpendTransactionTxOut.Value = send;

            uint256 sighash = swapSpendTransaction.GetSignatureHash(network, new Coin(swapTransaction, swapOutput.TxOut).ToScriptCoin(redeemScript));
            TransactionSignature signature = receiverPrivateKey.Sign(sighash, SigHash.All);

            swapSpentInput.ScriptSig = SwapScripts.GetAtomicSwapExchangeScriptSig(signature, sharedSecret, redeemScript);

            TransactionBuilder builder = new TransactionBuilder(network);
            builder.AddCoins(swapTransaction);
            var verifyresult = builder.Verify(swapSpendTransaction, out TransactionPolicyError[] result);

            if (result.Any())
            {
                StringBuilder sb = new();
                foreach (var policyError in result)
                {
                    sb.AppendLine(policyError.ToString());
                }

                throw new Exception(sb.ToString());
            }

            return swapSpendTransaction;
        }

        public static Transaction CreateSwapRecoveryTransaction(
            Network network,
            Transaction swapTransaction,
            Script redeemScript,
            Script sendToPublicKey,
            Key senderPrivateKey,
            FeeRate feeRate,
            DateTime lockTime)
        {
            var recoverTransaction = network.Consensus.ConsensusFactory.CreateTransaction();
            IndexedTxOut swapOutput = swapTransaction.Outputs.AsIndexedOutputs().Last(); // the swap is always the last output
            TxIn recoverInput = recoverTransaction.AddInput(swapTransaction, (int)swapOutput.N);
            TxOut recoverTransactionTxOut = recoverTransaction.AddOutput(Money.Coins(0), sendToPublicKey);

            Money fee = feeRate.GetFee(swapTransaction, network.Consensus.Options.WitnessScaleFactor);
            Money send = swapOutput.TxOut.Value - fee;
            recoverTransactionTxOut.Value = send;
            
            var locktime = Utils.DateTimeToUnixTime(lockTime);
            recoverTransaction.LockTime = locktime;
            recoverInput.Sequence = new Sequence(locktime);

            uint256 sighash = recoverTransaction.GetSignatureHash(network, new Coin(swapTransaction, swapOutput.TxOut).ToScriptCoin(redeemScript));
            TransactionSignature signature = senderPrivateKey.Sign(sighash, SigHash.All);

            recoverInput.ScriptSig = SwapScripts.GetAtomicSwapRecoverScriptSig(signature, redeemScript);

            TransactionBuilder builder = new TransactionBuilder(network);
            builder.AddCoins(swapTransaction);
            var verifyresult = builder.Verify(recoverTransaction, out TransactionPolicyError[] result);

            if (result.Any())
            {
                StringBuilder sb = new();
                foreach (var policyError in result)
                {
                    sb.AppendLine(policyError.ToString());
                }

                throw new Exception(sb.ToString());
            }

            return recoverTransaction;
        }
    }

    public class Utxo
    {
        public OutPoint OutPoint { get; set; }
        public Script Script { get; set; }
        public Money Amount { get; set; }
        public Key PrivateKey { get; set; }
    }
}