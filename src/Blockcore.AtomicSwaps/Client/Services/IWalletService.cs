using Blockcore.AtomicSwaps.Server.Controllers;

namespace Blockcore.AtomicSwaps.Client.Services
{
    /// <summary>
    /// A class that will handle communication with the indexers.
    /// </summary>
    public interface IWalletService
    {
        Task<string> ConnectWallet(WalletConnectInput walletConnectInput);

        Task<string> ConnectWallet(WalletAccounts walletAccounts);

    }
}