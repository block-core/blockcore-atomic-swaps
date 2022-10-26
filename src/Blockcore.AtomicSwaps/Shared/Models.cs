namespace Blockcore.AtomicSwaps.Server.Controllers;

public class SwapSessionCoin
{
    public string? OwnerPubkey { get; set; }
    public string? OwnerPubkeyAddress { get; set; }
    public string? OwnerPubkeyAddressChange { get; set; }
    public string? ReceiverPubkeyAddress{ get; set; }
    public string CoinSymbol { get; set; }
    public long Amount { get; set; }
    public string? SwapTransactionHex { get; set; }
    public string? SwapTransactionHash { get; set; }
    public string? RecoveryTransactionHex { get; set; }
    public string? ClaimTransactionHex { get; set; }
    public DateTime? RecoveryLockTime { get; set; }
}

public class SwapSession
{
    public string SwapSessionId { get; set; }
    public DateTime Created { get; set; }
    public SwapSessionCoin CoinSeller { get; set; }
    public SwapSessionCoin CoinBuyer { get; set; }
    public decimal ExchangeRate { get; set; }
    public string? SharedSecretHash { get; set; }
    public string Status { get; set; }
}

public class CreateSwapSession
{
    public string SwapSessionId { get; set; }
    public string OwnerPubkey { get; set; }
    public string FromCoinSymbol { get; set; }
    public string ToCoinSymbol { get; set; }
    public long AmountToSell { get; set; }
    public long AmountToBuy { get; set; }
    public string SharedSecretHash { get; set; }
}

public class AddressBalance
{
    public string address { get; set; }
    public long balance { get; set; }
    public long totalReceived { get; set; }
    public long totalStake { get; set; }
    public long totalMine { get; set; }
    public long totalSent { get; set; }
    public int totalReceivedCount { get; set; }
    public int totalSentCount { get; set; }
    public int totalStakeCount { get; set; }
    public int totalMineCount { get; set; }
    public long pendingSent { get; set; }
    public long pendingReceived { get; set; }
}


public class AccountInfo
{
    public string ExtPubKey { get; set; }
    public string Path { get; set; }
    public int LastFetchIndex { get; set; }
    public int LastFetchChangeIndex { get; set; }
    public long TotalBalance { get; set; }
    public Dictionary<string, List<UtxoData>> UtxoItems { get; set; } = new();
    public Dictionary<string, List<UtxoData>> UtxoChangeItems { get; set; } = new();

}

public class Outpoint
{
    public string transactionId { get; set; }
    public int outputIndex { get; set; }
}

public class UtxoData
{
    public Outpoint outpoint { get; set; }
    public string address { get; set; }
    public string scriptHex { get; set; }
    public long value { get; set; }
    public int blockIndex { get; set; }
    public bool coinBase { get; set; }
    public bool coinStake { get; set; }
    public string HdPath { get; set; }
}

public class Input
{
    public int inputIndex { get; set; }
    public string inputAddress { get; set; }
    public int inputAmount { get; set; }
    public string inputTransactionId { get; set; }
    public string scriptSig { get; set; }
    public string scriptSigAsm { get; set; }
    public string witScript { get; set; }
    public string sequenceLock { get; set; }
}

public class Output
{
    public string address { get; set; }
    public int balance { get; set; }
    public int index { get; set; }
    public string outputType { get; set; }
    public string scriptPubKeyAsm { get; set; }
    public string scriptPubKey { get; set; }
}

public class TransactionData
{
    public string symbol { get; set; }
    public string blockHash { get; set; }
    public int blockIndex { get; set; }
    public int timestamp { get; set; }
    public string transactionId { get; set; }
    public int transactionIndex { get; set; }
    public int confirmations { get; set; }
    public bool isCoinbase { get; set; }
    public bool isCoinstake { get; set; }
    public string lockTime { get; set; }
    public bool rbf { get; set; }
    public int version { get; set; }
    public int size { get; set; }
    public int virtualSize { get; set; }
    public int weight { get; set; }
    public int fee { get; set; }
    public bool hasWitness { get; set; }
    public List<Input> inputs { get; set; }
    public List<Output> outputs { get; set; }
}

