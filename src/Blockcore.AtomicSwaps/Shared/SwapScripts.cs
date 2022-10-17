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
        /// <summary>
        ///     OP_IF
        ///         OP_HASH160 <RIPEMD160(shared_secret)> OP_EQUALVERIFY
        ///         2 <owner_htlcpubkey> <receiver_htlcpubkey> 
        ///     OP_ELSE
        ///         2 <owner_htlcpubkey> <receiver_htlcpubkey> 
        ///     OP_ENDIF
        ///     2 OP_CHECKMULTISIG
        /// </summary>
        public byte[] GetAtomicSwapHtlcScript(
           ulong expirylocktime,
           PubKey ownerHtlckey,
           PubKey receiveHtlckey,
           uint256 sharedSecret)
        {
            byte[]? sharedSecretHash160 = NBitcoin.Crypto.Hashes.RIPEMD160(sharedSecret.ToBytes().ToArray());

            List<Op> ops = new List<Op>
            {
                OpcodeType.OP_IF,
                OpcodeType.OP_HASH160,
                Op.GetPushOp(sharedSecretHash160),
                OpcodeType.OP_EQUALVERIFY,
                Op.GetPushOp(2),
                Op.GetPushOp(ownerHtlckey.ToBytes()),
                Op.GetPushOp(receiveHtlckey.ToBytes()),
                OpcodeType.OP_ELSE,
                Op.GetPushOp(2),
                Op.GetPushOp(ownerHtlckey.ToBytes()),
                Op.GetPushOp(receiveHtlckey.ToBytes()),
                OpcodeType.OP_ENDIF,
                Op.GetPushOp(2),
                OpcodeType.OP_CHECKMULTISIG,
            };

            var script = new Script(ops);
            return script.ToBytes();
        }

        public byte[] GetAtomicSwapRecoveryTransaction(int hours)
        {
            Transaction transaction = new Transaction();

            transaction.LockTime = new LockTime(DateTime.UtcNow.AddHours(hours));

            return transaction.ToBytes();
        }

        /// <summary>
        ///   TODO: This can be improved look at doing this instead, it will save an additional request for a signature
        ///   option of the time lock in the script itself (this means no need for a exit trx)
        /// 
        ///     OP_IF
        ///         OP_HASH160 <RIPEMD160(shared_secret)> OP_EQUALVERIFY
        ///          2 <owner_htlcpubkey> <receiver_htlcpubkey> 2 OP_CHECKMULTISIG
        ///     OP_ELSE
        ///          OP_DROP <expiry_locktime> OP_CHECKLOCKTIMEVERIFY OP_DROP
        ///          OP_CHECKSIG <owner_htlcpubkey>
        ///      OP_ENDIF
        /// 
        /// </summary>
        public byte[] GetAtomicSwapHtlcExpieryScript(
            ulong expirylocktime,
            PubKey ownerHtlckey,
            PubKey receiveHtlckey,
            uint256 sharedSecret)
        {
            byte[]? sharedSecretHash160 = NBitcoin.Crypto.Hashes.RIPEMD160(sharedSecret.ToBytes().ToArray());

            List<Op> ops = new List<Op>
            {
                OpcodeType.OP_IF,
                OpcodeType.OP_HASH160,
                Op.GetPushOp(sharedSecretHash160),
                OpcodeType.OP_EQUALVERIFY,
                Op.GetPushOp(2),
                Op.GetPushOp(ownerHtlckey.ToBytes()),
                Op.GetPushOp(receiveHtlckey.ToBytes()),
                Op.GetPushOp(2),
                OpcodeType.OP_CHECKMULTISIG,
                OpcodeType.OP_ELSE,
                OpcodeType.OP_DROP,
                Op.GetPushOp((long)expirylocktime),
                OpcodeType.OP_CHECKLOCKTIMEVERIFY,
                OpcodeType.OP_DROP,
                OpcodeType.OP_CHECKSIG,
                Op.GetPushOp(ownerHtlckey.ToBytes()),
                OpcodeType.OP_ENDIF,
            };

            var script = new Script(ops);
            return script.ToBytes();
        }
    }
}