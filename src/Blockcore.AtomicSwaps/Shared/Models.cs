namespace Blockcore.AtomicSwaps.Server.Controllers;

public class SwapSessionCoin
{
    public string CoinSymbol { get; set; }
    public long Amount { get; set; }
    public string SwapTransactionHex { get; set; }
    public string RecoveryTransactionHex { get; set; }
    public string ClaimTransactionHex { get; set; }
    public DateTime RecoveryLockTime { get; set; }
}

public class SwapSession
{
    public string SwapSessionId { get; set; }
    public DateTime Created { get; set; }
    public SwapSessionCoin CoinSeller { get; set; }
    public SwapSessionCoin CoinBuyer { get; set; }
    public decimal ExchangeRate { get; set; }
    public string SharedSecretHash { get; set; }
    public string Status { get; set; }
}

public class CreateSwapSession
{
    public string FromCoinSymbol { get; set; }
    public string ToCoinSymbol { get; set; }

    public long AmountToSell { get; set; }

    public long AmountToBuy { get; set; }

}