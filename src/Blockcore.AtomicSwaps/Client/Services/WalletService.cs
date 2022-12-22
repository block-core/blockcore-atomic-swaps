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

        public async Task<string> ConnectWallet(WalletAccounts walletAccounts)
        {
            try
            {
                var res = await _walletConnector.GetWallet();
                _logger.LogInformation(res);

                var walletApiMessage = JsonSerializer.Deserialize<WalletApiMessage?>(res);

                if (walletApiMessage == null)
                {
                    return $"Failed to read wallet data";
                }

                if (walletAccounts.Connected)
                {
                    if (walletAccounts.WalletPubKey != walletApiMessage.key)
                    {
                        return $"Incorrect wallet, expected {walletAccounts.WalletPubKey} but got {walletApiMessage.key}";
                    }
                }
                else
                {
                    walletAccounts.WalletPubKey = walletApiMessage.key;
                }

                foreach (var walletAccount in walletApiMessage.content.accounts)
                {
                    if (walletAccounts.Accounts.TryGetValue(walletAccount.networkType, out WalletAccount account))
                    {
                        // refresh the balance
                        account.Balance = walletAccount.history.balance;
                    }
                    else
                    {
                        // based on BCIP2 and BCIP3 "m / purpose' / coin_type' / account' / swap_key' / secret"
                        // the wallet key already derives ""m / purpose'" so we derive eh account key next
                        var keysRes = await _walletConnector.GetSwapKey(walletApiMessage.key, walletApiMessage.content.wallet.id, walletAccount.id, false);
                        var keys = JsonSerializer.Deserialize<WalletApiMessage<WalletApiMessageKeys>>(keysRes);

                        WalletAccount newAccount = new WalletAccount
                        {
                            Pubkey = keys.response.publicKey,
                            CoinSymbol = walletAccount.networkType,
                            WalletId = walletApiMessage.content.wallet.id,
                            AccountId = walletAccount.id,
                            Balance = walletAccount.history.balance,
                        };

                        walletAccounts.Accounts.Add(newAccount.CoinSymbol, newAccount);
                    }
                }

                _storage.Set<WalletAccounts>(walletAccounts);
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
