using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.Client.Models;
using Blockcore.AtomicSwaps.MetaMask;
using Microsoft.AspNetCore.Components;


namespace Blockcore.AtomicSwaps.Client.Pages
{

    public partial class BlockcoreWallet : IDisposable
    {
        private bool disposedValue;

        [Inject]
        public IBlockcoreWalletService BlockcoreWalletService { get; set; } = default!;
        public bool HasBlockcoreWallet { get; set; }

        protected override async Task OnInitializedAsync()
        {
           HasBlockcoreWallet = await BlockcoreWalletService.HasBlockcoreWallet();
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BlockcoreWallet()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
