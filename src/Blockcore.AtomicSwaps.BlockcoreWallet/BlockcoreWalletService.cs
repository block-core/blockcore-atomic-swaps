using Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions;
using Microsoft.JSInterop;
using System;
using System.Numerics;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreWallet
{
	// This class provides JavaScript functionality for BlockcoreWallet wrapped
	// in a .NET class for easy consumption. The associated JavaScript module is
	// loaded on demand when first needed.
	//
	// This class can be registered as scoped DI service and then injected into Blazor
	// components for use.

	public class BlockcoreWalletService : IAsyncDisposable, IBlockcoreWalletService
	{
		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		//public static event Func<Task>? ConnectEvent;
		//public static event Func<Task>? DisconnectEvent;

		public BlockcoreWalletService(IJSRuntime jsRuntime)
		{
			moduleTask = new(() => LoadScripts(jsRuntime).AsTask());
		}

		public ValueTask<IJSObjectReference> LoadScripts(IJSRuntime jsRuntime)
		{
			return jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blockcore.AtomicSwaps.BlockcoreWallet/blockcoreWallet.js");
		}

		public async ValueTask ConnectBlockcoreWallet()
		{
			var module = await moduleTask.Value;
			try
			{
				await module.InvokeVoidAsync("checkBlockcoreWallet");
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask<bool> HasBlockcoreWallet()
		{
			var module = await moduleTask.Value;
			try
			{
				return await module.InvokeAsync<bool>("hasBlockcoreWallet");
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask<bool> IsSiteConnected()
		{
			var module = await moduleTask.Value;
			try
			{
				return await module.InvokeAsync<bool>("isSiteConnected");
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask<string> SignMessageAnyAccount(string value)
		{
			var module = await moduleTask.Value;
			try
			{
				return await module.InvokeAsync<string>("signMessageAnyAccount", value);
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask<string> SignMessageAnyAccountJson(string value)
		{
			var module = await moduleTask.Value;
			try
			{
				return await module.InvokeAsync<string>("signMessageAnyAccountJson", value);
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask<string> SignMessage(string msg)
		{
			var module = await moduleTask.Value;
			try
			{
				return await module.InvokeAsync<string>("signMessage", msg);
			}
			catch (Exception ex)
			{
				HandleExceptions(ex);
				throw;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (moduleTask.IsValueCreated)
			{
				var module = await moduleTask.Value;
				await module.DisposeAsync();
			}
		}

		private void HandleExceptions(Exception ex)
		{
			switch (ex.Message)
			{
				case "NoBlockcoreWallet":
					throw new NoBlockcoreWalletException();
				case "UserDenied":
					throw new UserDeniedException();
			}
		}


	}
}
