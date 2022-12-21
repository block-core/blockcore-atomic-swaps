using Microsoft.JSInterop;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreWallet
{
	public interface IBlockcoreWalletService
	{

		ValueTask ConnectBlockcoreWallet();
		ValueTask DisposeAsync();
		ValueTask<bool> HasBlockcoreWallet();
		ValueTask<bool> IsSiteConnected();
		ValueTask<string> SignMessageAnyAccount(string value);
        ValueTask<string> GetWallet(string? key = null);
        ValueTask<string> GetSwapKey(string key, string walletId, string accountId, bool includePrivateKey);
        ValueTask<string> GetSwapSecret(string key, string walletId, string accountId, string message);
        ValueTask<string> SignMessageAnyAccountJson(string value);
		ValueTask<string> PaymentRequest(string network, string amount);
		ValueTask<string> DIDSupportedMethods();
		ValueTask<string> DIDRequest(string[] methods);
		ValueTask<string> SignMessage(string msg);

	}
}