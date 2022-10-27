using Blazored.LocalStorage;
using Blockcore.AtomicSwaps.Client;
using Blockcore.AtomicSwaps.Client.Logging;
using Blockcore.AtomicSwaps.MetaMask;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.Configure<WebApiLoggerOptions>(options => builder.Configuration.GetSection("WebApiLogger").Bind(options));
builder.Services.AddLogging(configure =>
{
    configure.AddWebApiLogger();
});
builder.Services.AddSingleton(sp => new GlobalData ());

builder.Services.AddScoped<Storage>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddMetaMask();

await builder.Build().RunAsync();
