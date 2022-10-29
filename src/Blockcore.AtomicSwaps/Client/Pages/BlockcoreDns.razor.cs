using Blockcore.AtomicSwaps.BlockcoreDns;
using Microsoft.AspNetCore.Components;


namespace Blockcore.AtomicSwaps.Client.Pages
{
    public partial class BlockcoreDns : IDisposable
    {
        private bool disposedValue;
        [Inject]
        public IBlockcoreDnsService blockcoreDnsService { get; set; } = default!;
        public string? blockcoreDnsUrl { get; set; }

        protected override Task OnInitializedAsync()
        {
            blockcoreDnsUrl = blockcoreDnsService.LoadUrl();
            return Task.CompletedTask;
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
