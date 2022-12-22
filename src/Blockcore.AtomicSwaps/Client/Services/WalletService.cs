using Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions;
using Blockcore.AtomicSwaps.Client.Pages;
using Blockcore.AtomicSwaps.Server.Controllers;
using System.Net.Http.Json;
using System.Text.Json;
using Blockcore.AtomicSwaps.BlockcoreWallet;

namespace Blockcore.AtomicSwaps.Client.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger<BlockchainApiService> _logger;
        private readonly Storage _storage;
        private readonly SwapsConfiguration _swapsConfiguration;
        private readonly IBlockcoreWalletConnector _walletConnector;

        public WalletService(ILogger<BlockchainApiService> logger, Storage storage, SwapsConfiguration swapsConfiguration, IBlockcoreWalletConnector walletConnector)
        {
            _logger = logger;
            _storage = storage;
            _swapsConfiguration = swapsConfiguration;
            _walletConnector = walletConnector;
        }

        public Task<string> ConnectWallet(WalletAccounts walletAccounts)
        {
            return ConnectWallet(new WalletConnectInput { WalletAccounts = walletAccounts });
        }

        public async Task<string> ConnectWallet(WalletConnectInput walletConnectInput)
        {
            try
            {
                var res = await _walletConnector.GetWallet();
                _logger.LogInformation(res);

                walletConnectInput.WalletApiMessage = JsonSerializer.Deserialize<WalletApiMessage?>(res);

                if (walletConnectInput.WalletApiMessage == null)
                {
                    return $"Failed to read wallet data";
                }

                if (walletConnectInput.WalletAccounts.Connected)
                {
                    if (walletConnectInput.WalletAccounts.WalletPubKey != walletConnectInput.WalletApiMessage.key)
                    {
                        return $"Incorrect wallet, expected {walletConnectInput.WalletAccounts.WalletPubKey} but got {walletConnectInput.WalletApiMessage.key}";
                    }
                }
                else
                {
                    walletConnectInput.WalletAccounts.WalletPubKey = walletConnectInput.WalletApiMessage.key;
                }

                foreach (var walletAccount in walletConnectInput.WalletApiMessage.content.accounts)
                {
                    if (walletConnectInput.WalletAccounts.Accounts.TryGetValue(walletAccount.networkType, out WalletAccount account))
                    {
                        // refresh the balance
                        account.Balance = walletAccount.history.balance;
                    }
                    else
                    {
                        // based on BCIP2 and BCIP3 "m / purpose' / coin_type' / account' / swap_key' / secret"
                        // the wallet key already derives ""m / purpose'" so we derive eh account key next
                        var keysRes = await _walletConnector.GetSwapKey(walletConnectInput.WalletApiMessage.key, walletConnectInput.WalletApiMessage.content.wallet.id, walletAccount.id, false);
                        var keys = JsonSerializer.Deserialize<WalletApiMessage<WalletApiMessageKeys>>(keysRes);

                        WalletAccount newAccount = new WalletAccount
                        {
                            Pubkey = keys.response.publicKey,
                            CoinSymbol = walletAccount.networkType,
                            WalletId = walletConnectInput.WalletApiMessage.content.wallet.id,
                            AccountId = walletAccount.id,
                            Balance = walletAccount.history.balance,
                        };

                        walletConnectInput.WalletAccounts.Accounts.Add(newAccount.CoinSymbol, newAccount);
                    }
                }

                _storage.Set<WalletAccounts>(walletConnectInput.WalletAccounts);
            }
            catch (NoBlockcoreWalletException nbwe)
            {
                return "Not wallet found please install the wallet at blockcore.net";
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return e.Message;
            }

            return string.Empty;
        }
    }
}
