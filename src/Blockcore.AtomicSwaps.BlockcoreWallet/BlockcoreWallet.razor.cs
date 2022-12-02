using System;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.BlockcoreWallet.Exceptions;
using Microsoft.AspNetCore.Components;


namespace Blockcore.AtomicSwaps.BlockcoreWallet
{

	public partial class BlockcoreWallet : IDisposable
	{
		private bool disposedValue;

		[Inject]
		public IBlockcoreWalletService blockcoreWalletService { get; set; } = default!;
		public bool HasBlockcoreWallet { get; set; }
		public string? SignedMessage { get; set; }

		protected override async Task OnInitializedAsync()
		{
			HasBlockcoreWallet = await blockcoreWalletService.HasBlockcoreWallet();
		}

		public async Task SignMessage(string message)
		{
			try
			{
				var result = await blockcoreWalletService.SignMessage(message);
				SignedMessage = $"Signed: {result}";
			}
			catch (UserDeniedException)
			{
				SignedMessage = "User Denied";
			}
			catch (Exception ex)
			{
				SignedMessage = $"Exception: {ex}";
			}
		}




		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}



		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}


}
