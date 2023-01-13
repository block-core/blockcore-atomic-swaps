namespace Blockcore.AtomicSwaps.Server.Controllers;

public class SwapSessionCoin
{
    public string? SenderPubkey { get; set; }
    public string? ReceiverPubkey { get; set; }
    public string CoinSymbol { get; set; }
    public long Amount { get; set; }
    public long NetworkFee { get; set; }
    public long ServiceFee { get; set; }
    public string? ServiceAddress { get; set; }
    public string? SwapTransactionHex { get; set; }
    public string? SwapRedeemScriptHex { get; set; }
    public string? SwapTransactionHash { get; set; }
    public string? SenderRecoveryTransactionHex { get; set; }
    public string? SenderRecoveryTransactionHash { get; set; }
    public string? ExchangeTransactionHex { get; set; }
    public string? ExchangeTransactionHash { get; set; }
    public DateTime? RecoveryLockTime { get; set; }
    public string? SwapAddress { get; set; }
}

public class SwapSession
{
    public string SwapSessionId { get; set; }
    public DateTime Created { get; set; }
    public SwapSessionCoin CoinSeller { get; set; } = new();
    public SwapSessionCoin CoinBuyer { get; set; } = new();
    public decimal ExchangeRate { get; set; }
    public string? SharedSecretHash { get; set; }
    public string? SharedSecret { get; set; }
    public string Status { get; set; }
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
    public string hdPath { get; set; }
}

public class Input
{
    public int inputIndex { get; set; }
    public string inputAddress { get; set; }
    public long inputAmount { get; set; }
    public string inputTransactionId { get; set; }
    public string scriptSig { get; set; }
    public string scriptSigAsm { get; set; }
    public string witScript { get; set; }
    public string sequenceLock { get; set; }
}

public class Output
{
    public string address { get; set; }
    public long balance { get; set; }
    public int index { get; set; }
    public string outputType { get; set; }
    public string scriptPubKeyAsm { get; set; }
    public string scriptPubKey { get; set; }
    public string? spentInTransaction { get; set; }
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





public class WalletAccounts
{
    public Dictionary<string, WalletAccount> Accounts { get; set; } = new();

    /// <summary>
    /// BCIP3 wallet key.
    /// </summary>
    public string WalletPubKey { get; set; }

    /// <summary>
    /// If the WalletPubKey exists then we have connected to a wallet in the past.
    /// </summary>
    public bool Connected => !string.IsNullOrEmpty(WalletPubKey);

    public bool HasAccountKey(string pubkey)
    {
	    return Accounts.Values.Any(a => a.Pubkey == pubkey);
    }

    public WalletAccount GetAccount(string coinSymbol)
    {
        return Accounts[coinSymbol];
    }

    public WalletAccount? TryGetAccount(string coinSymbol)
    {
        Accounts.TryGetValue(coinSymbol, out WalletAccount? account);
        
        return account;
    }
}

public class WalletAccount
{
    public string Pubkey { get; set; }
    public string Address { get; set; }
    public string PubkeyPath { get; set; }
    public string CoinSymbol { get; set; }
    public long Balance { get; set; }
    public string WalletId { get; set; }
    public string AccountId { get; set; }

    // We need this to know if this is a P2SH or a P2WSH account
    public int AccountPurpose { get; set; }
}

public class WalletResultMessage<T> where T: class
{
    public string key { get; set; }
    public string signature { get; set; }
    public T response { get; set; }
    public string content { get; set; }
    public string network { get; set; }
    public string walletId { get; set; }
    public string accountId { get; set; }
}


public class WalletApiMessageKeys
{
    public string publicKey { get; set; }
    public string privateKey { get; set; }
}

public class WalletApiMessageSecret
{
    public string secret { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
