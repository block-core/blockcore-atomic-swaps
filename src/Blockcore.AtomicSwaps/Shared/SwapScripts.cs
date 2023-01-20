using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using NBitcoin;

namespace Blockcore.AtomicSwaps.Shared
{
    /// <summary>
    /// Atomic Swap HTLC
    /// </summary>
    public class SwapScripts
    {
        public static uint256 GetSecretFromScriptSig(Transaction transaciton)
        {
            byte[] secretBytes = null;

            if (transaciton.Inputs[0].WitScript != null)
            {
                secretBytes = transaciton.Inputs[0].WitScript.ToScript().ToOps()[1].PushData;
            }
            else
            {
                secretBytes = transaciton.Inputs[0].ScriptSig.ToOps()[1].PushData;
            }

            var secret = new uint256(secretBytes);

            return secret;
        }

        public static Script GetAtomicSwapExchangeScriptSig(TransactionSignature senderSig, uint256 sharedSecret, Script redeemScript)
        {
            var exchangeRedeemScript = new Script(
                Op.GetPushOp(senderSig.ToBytes()),     // put the senders signature on the stack
                Op.GetPushOp(sharedSecret.ToBytes()),           // put the shared secret on the stack
                Op.GetPushOp(1),                                // go down the if == true condition
                Op.GetPushOp(redeemScript.ToBytes()));          // the redeem script

            return exchangeRedeemScript;
        }
        public static Script GetAtomicSwapRecoverScriptSig(TransactionSignature receiverSig, Script redeemScript)
        {
            var recoverRedeemScript = new Script(
                Op.GetPushOp(receiverSig.ToBytes()),   // put the receivers signature on the stack
                Op.GetPushOp(0),                                // go down the if === false (the else path) condition
                Op.GetPushOp(redeemScript.ToBytes()));          // the redeem script

            return recoverRedeemScript;
        }

        /// <summary>
        /// This is the atomic swap script, the logic here is as following:
        /// IF TRUE path - if the receiver knows the shared-secret they can
        /// spend the coins together with their private key, by spending the
        /// coins they have to reveal the shared-secret
        /// ELSE path - the sender (owner) of the coins can take back using
        /// their private key after the lock time has passed.
        /// 
        ///     OP_IF
        ///         OP_HASH160<H(shared_secret)>
        ///         OP_EQUALVERIFY
        ///         <receiver-key>
        ///         OP_CHECKSIG
        ///     OP_ELSE
        ///         <time_lock>
        ///         OP_CHECKLOCKTIMEVERIFY
        ///         OP_DROP
        ///         <sender-key>
        ///         OP_CHECKSIG       
        ///     OP_ENDIF
        /// 
        /// </summary>
        public static Script GetAtomicSwapHtlcScript(
            ulong cltvTimelock,
            PubKey senderKey,
            PubKey receiverKey,
            uint160 sharedSecretHash)
        {
            List<Op> ops = new List<Op>
            {
                OpcodeType.OP_IF,
                    OpcodeType.OP_HASH160, 
                    Op.GetPushOp(sharedSecretHash.ToBytes()), 
                    OpcodeType.OP_EQUALVERIFY,
                    Op.GetPushOp(receiverKey.ToBytes()), 
                    OpcodeType.OP_CHECKSIG,
                OpcodeType.OP_ELSE,
                    Op.GetPushOp((long)cltvTimelock), 
                    OpcodeType.OP_CHECKLOCKTIMEVERIFY, 
                    OpcodeType.OP_DROP, 
                    Op.GetPushOp(senderKey.ToBytes()), 
                    OpcodeType.OP_CHECKSIG,
                OpcodeType.OP_ENDIF,
            };

            var script = new Script(ops);
            return script;
        }
    }
}