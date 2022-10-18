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
       
        public byte[] GetAtomicSwapRecoveryTransaction(int hours)
        {
            Transaction transaction = new Transaction();

            transaction.LockTime = new LockTime(DateTime.UtcNow.AddHours(hours));

            return transaction.ToBytes();
        }
      
        /// <summary>
        /// 
        ///    OP_IF
        ///         <sender-key> OP_CHECKSIGVERIFY <time_lock> OP_CHECKLOCKTIMEVERIFY
        ///     OP_ELSE
        ///         <receiver-key> OP_CHECKSIGVERIFY OP_HASH160<H(shared_secret)> OP_EQUAL
        ///     OP_ENDIF
        /// 
        /// </summary>
        public Script GetAtomicSwapHtlcScript(
            ulong cltvTimelock,
            PubKey senderKey,
            PubKey receiverKey,
            uint256 sharedSecret)
        {
            byte[]? sharedSecretHash160 = NBitcoin.Crypto.Hashes.RIPEMD160(sharedSecret.ToBytes().ToArray());

            List<Op> ops = new List<Op>
            {
                OpcodeType.OP_IF,
                      Op.GetPushOp(senderKey.ToBytes()), OpcodeType.OP_CHECKSIGVERIFY, Op.GetPushOp((long)cltvTimelock), OpcodeType.OP_CHECKLOCKTIMEVERIFY,
                OpcodeType.OP_ELSE,
                      Op.GetPushOp(receiverKey.ToBytes()), OpcodeType.OP_CHECKSIGVERIFY, OpcodeType.OP_HASH160, Op.GetPushOp(sharedSecretHash160), OpcodeType.OP_EQUALVERIFY,
                OpcodeType.OP_ENDIF,
            };

            var script = new Script(ops);
            return script;
        }
    }
}