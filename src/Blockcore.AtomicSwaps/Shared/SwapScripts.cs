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
       
        public static Script GetAtomicSwapExchangeScriptSig(TransactionSignature senderSig, uint256 sharedSecret)
        {
            var redeemScript = new Script(
                Op.GetPushOp(senderSig.ToBytes()),
                Op.GetPushOp(sharedSecret.ToBytes()),
                Op.GetPushOp(1));

            return redeemScript;
        }
        public static Script GetAtomicSwapRecoverScriptSig(TransactionSignature receiverSig)
        {
            var redeemScript = new Script(
                Op.GetPushOp(receiverSig.ToBytes()), 
                Op.GetPushOp(0));

            return redeemScript;
        }

        /// <summary>
        /// 
        ///     OP_IF
        ///         OP_HASH160<H(shared_secret)> OP_EQUALVERIFY <receiver-key> OP_CHECKSIG
        ///     OP_ELSE
        ///         <time_lock> OP_CHECKLOCKTIMEVERIFY OP_DROP <sender-key> OP_CHECKSIG       
        ///     OP_ENDIF
        /// 
        /// </summary>
        public static Script GetAtomicSwapHtlcScript(
            ulong cltvTimelock,
            PubKey senderKey,
            PubKey receiverKey,
            uint256 sharedSecret)
        {
            byte[]? sharedSecretHash160 = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes()).ToBytes();

            List<Op> ops = new List<Op>
            {
                OpcodeType.OP_IF,
                    OpcodeType.OP_HASH160, Op.GetPushOp(sharedSecretHash160), OpcodeType.OP_EQUALVERIFY,Op.GetPushOp(receiverKey.ToBytes()), OpcodeType.OP_CHECKSIG,
                OpcodeType.OP_ELSE,
                    Op.GetPushOp((long)cltvTimelock), OpcodeType.OP_CHECKLOCKTIMEVERIFY, OpcodeType.OP_DROP, Op.GetPushOp(senderKey.ToBytes()), OpcodeType.OP_CHECKSIG,
                OpcodeType.OP_ENDIF,
            };

            var script = new Script(ops);
            return script;
        }
    }
}