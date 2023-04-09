using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.BlockcoreDns;
using Blockcore.AtomicSwaps.Client;
using Blockcore.AtomicSwaps.Client.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Blockcore.AtomicSwaps.Client.Services.UserPreferences;
using Blockcore.AtomicSwaps.Client.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Hosting;
using Blockcore.AtomicSwaps.Client.HostedServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.Configure<WebApiLoggerOptions>(options => builder.Configuration.GetSection("WebApiLogger").Bind(options));
builder.Services.AddLogging(configure =>
{
    configure.AddWebApiLogger();
});
builder.Services.AddSingleton(sp => new SwapsConfiguration());

builder.Services.AddScoped<Storage>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();

builder.Services.AddScoped<LayoutService>();

builder.Services.AddScoped<IBlockchainApiService, BlockchainApiService>();

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<ISnackbarService, SnackbarService>();

builder.Services.AddBlockcoreWallet();

builder.Services.AddBlockcoreDns();

builder.Services.AddMudServices();

builder.Services.AddHostedService<UpdateDataFromDnsHostedService>();

builder.Services.AddSingleton<UpdateDataFromDnsHostedService>();

builder.Services.AddSingleton<PeriodicExecutorService>();

await builder.Build().RunAsync();
