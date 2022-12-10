using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.BlockcoreWallet;
using Blockcore.AtomicSwaps.BlockcoreDns;
using Blockcore.AtomicSwaps.Client;
using Blockcore.AtomicSwaps.Client.Logging;
using Blockcore.AtomicSwaps.MetaMask;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Blockcore.AtomicSwaps.Client.Services.UserPreferences;
using Blockcore.AtomicSwaps.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.Configure<WebApiLoggerOptions>(options => builder.Configuration.GetSection("WebApiLogger").Bind(options));
builder.Services.AddLogging(configure =>
{
	configure.AddWebApiLogger();
});
builder.Services.AddSingleton(sp => new GlobalData());

builder.Services.AddScoped<Storage>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();

builder.Services.AddScoped<LayoutService>();

builder.Services.AddMetaMask();

builder.Services.AddBlockcoreWallet();

builder.Services.AddBlockcoreDns();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
