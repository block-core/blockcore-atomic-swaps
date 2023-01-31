using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions;
using Blockcore.AtomicSwaps.Server.Controllers;
using Blockcore.Utilities;
using NBitcoin;
using System.Text.Json;

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

        public async Task<BlockcoreWalletSendFundsOut?> SendCoins(BlockcoreWalletSendFunds blockcoreWalletSendFunds)
        {
            try
            {
                var res = await _walletConnector.SendCoins(blockcoreWalletSendFunds);
                return res;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }


        public async Task<BlockcoreWalletSwapCoinsOut?> SwapCoins(BlockcoreWalletSwapCoins blockcoreWalletSwapCoins)
        {
            try
            {
                var res = await _walletConnector.SwapCoins(blockcoreWalletSwapCoins);
                return res;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<string> ConnectWallet(WalletConnectInput walletConnectInput)
        {
            try
            {
                walletConnectInput.WalletApiMessage = await _walletConnector.GetWallet();

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

                foreach (var walletAccount in walletConnectInput.WalletApiMessage.response.accounts)
                {
                    if (walletConnectInput.WalletAccounts.Accounts.TryGetValue(walletAccount.id, out WalletAccount account))
                    {
                        // refresh the balance
                        account.Balance = walletAccount.history.balance;
                        account.Name = walletAccount.name;
                    }
                    else
                    {
                        // based on BCIP2 and BCIP3 "m / purpose' / coin_type' / account' / swap_key' / secret"
                        // the wallet key already derives ""m / purpose'" so we derive eh account key next
                        var keysRes = await _walletConnector.GetSwapKey(walletConnectInput.WalletApiMessage.key, walletConnectInput.WalletApiMessage.response.wallet.id, walletAccount.id, false);
                        var keys = JsonSerializer.Deserialize<WalletResultMessage<WalletApiMessageKeys>>(keysRes);

                        WalletAccount newAccount = new WalletAccount
                        {
                            Pubkey = keys.response.publicKey,
                            Name = walletAccount.name,
                            CoinSymbol = walletAccount.networkType,
                            WalletId = walletConnectInput.WalletApiMessage.response.wallet.id,
                            AccountId = walletAccount.id,
                            Balance = walletAccount.history.balance,
                            AccountPurpose = walletAccount.purpose
                        };

                        walletConnectInput.WalletAccounts.Accounts.Add(newAccount.AccountId, newAccount);
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

        public async Task<(string? Error, uint256? Secret, uint160? SecretHash)> GenerateSecretHash(WalletAccount walletAccount, string sessionId)
        {
            Guard.NotNull(walletAccount, nameof(walletAccount));
            Guard.NotEmpty(sessionId, nameof(sessionId));

            try
            {
                var res = await _walletConnector.GetSwapSecret(walletAccount.Pubkey, walletAccount.WalletId, walletAccount.AccountId, sessionId);
                _logger.LogInformation(res);

                var data = JsonSerializer.Deserialize<WalletResultMessage<WalletApiMessageSecret>>(res);

                if (data?.response?.secret == null)
                {
                    return ($"Failed to generate the secret", null, null);
                }

                var sharedSecret = new uint256(data.response.secret);//  SwapsConfiguration.GenerateSecret(data.signature, sessionId);
                var sharedSecretHash = NBitcoin.Crypto.Hashes.Hash160(sharedSecret.ToBytes());

                return (null, sharedSecret, sharedSecretHash);
            }
            catch (NoBlockcoreWalletException nbwe)
            {
                return ("Not wallet found please install the wallet at blockcore.net", null, null);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (e.Message, null, null);

            }
        }
    }
}
