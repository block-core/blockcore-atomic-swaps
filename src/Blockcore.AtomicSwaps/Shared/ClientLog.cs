using Microsoft.Extensions.Logging;

namespace Blockcore.AtomicSwaps.Shared
{
    public class ClientLog
    {
        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

        public string StackTrace { get; set; }

        public string Url { get; set; }
    }
}