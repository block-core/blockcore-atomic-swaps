using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blockcore.AtomicSwaps.Client.Logging
{
    public class ClientLoggerProvider : ILoggerProvider
    {
        private readonly HttpClient _httpClient;
        private readonly WebApiLoggerOptions _options;
        private readonly NavigationManager _navigationManager;

        public ClientLoggerProvider(
                IServiceProvider serviceProvider,
                IOptions<WebApiLoggerOptions> options,
                NavigationManager navigationManager)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _httpClient = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<HttpClient>();
            _options = options.Value;
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new WebApiLogger(_httpClient, _options, _navigationManager);
        }

        public void Dispose()
        {
        }
    }
}