using System;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreDns
{


    public class BlockcoreDnsService : IAsyncDisposable, IBlockcoreDnsService
    {
        private readonly string _Url = "https://chains.blockcore.net/services/DNS.json";

        public BlockcoreDnsService()
        {
        }

        public string LoadUrl()
        {
            return _Url;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
