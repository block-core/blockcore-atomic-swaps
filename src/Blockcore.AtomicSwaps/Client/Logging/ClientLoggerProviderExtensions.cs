using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blockcore.AtomicSwaps.Client.Logging
{
    public static class ClientLoggerProviderExtensions
    {
        public static ILoggingBuilder AddWebApiLogger(this ILoggingBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILoggerProvider, ClientLoggerProvider>();
            return builder;
        }
    }
}