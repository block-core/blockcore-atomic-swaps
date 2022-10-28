using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Blockcore.AtomicSwaps.BlockcoreWallet
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBlockcoreWallet(this IServiceCollection services)
        {
            services.AddScoped<IBlockcoreWalletService>(sp => new BlockcoreWalletService(sp.GetRequiredService<IJSRuntime>()));
        }
    }
}
