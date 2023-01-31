using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.Server.Controllers;
using NBitcoin;

namespace Blockcore.AtomicSwaps.Client.Services
{
    /// <summary>
    /// A class that will handle communication with the indexers.
    /// </summary>
    public interface IWalletService
    {
        Task<string> ConnectWallet(WalletConnectInput walletConnectInput);

        Task<string> ConnectWallet(WalletAccounts walletAccounts);

        Task<BlockcoreWalletSendFundsOut?> SendCoins(BlockcoreWalletSendFunds blockcoreWalletSendFunds);

        Task<BlockcoreWalletSwapCoinsOut?> SwapCoins(BlockcoreWalletSwapCoins blockcoreWalletSwapCoins);

        Task<(string? Error, uint256? Secret, uint160? SecretHash)> GenerateSecretHash(WalletAccount walletAccount, string sessionId);
    }

    public class WalletConnectInput
    {
        public WalletAccounts WalletAccounts { get; set; }
        public BlockcoreWalletMessageOut? WalletApiMessage { get; set; }
    }
}