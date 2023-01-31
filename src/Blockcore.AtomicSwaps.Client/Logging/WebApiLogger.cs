using System;
using System.Net.Http;
using System.Net.Http.Json;
using Blockcore.AtomicSwaps.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blockcore.AtomicSwaps.Client.Logging
{
    public class WebApiLogger : ILogger
    {
        private readonly WebApiLoggerOptions _options;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;

        public WebApiLogger(HttpClient httpClient, WebApiLoggerOptions options, NavigationManager navigationManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _options.LogLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            try
            {
                ClientLog log = new()
                {
                    LogLevel = logLevel,
                    EventId = eventId,
                    Message = formatter(state, exception),
                    Exception = exception?.Message,
                    StackTrace = exception?.StackTrace,
                    Url = _navigationManager.Uri
                };
              _httpClient.PostAsJsonAsync(_options.LoggerEndpointUrl, log);

            }
            catch(Exception ex)
            {
                // don't throw exceptions from the logger
            }
        }
    }
}