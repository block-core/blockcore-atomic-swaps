using Microsoft.Extensions.Logging;

namespace Blockcore.AtomicSwaps.Client.Logging
{
    public class WebApiLoggerOptions
    {
        public string LoggerEndpointUrl { set; get; }

        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}