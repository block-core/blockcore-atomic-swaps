using Microsoft.Extensions.Hosting;

namespace Blockcore.AtomicSwaps.Client.HostedServices
{
    public class UpdateDataFromDnsHostedService : BackgroundService
    {
        private readonly ILogger<UpdateDataFromDnsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Storage _storage;

        public UpdateDataFromDnsHostedService(IServiceProvider serviceProvider, ILogger<UpdateDataFromDnsHostedService> logger, Storage storage)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _storage = storage;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        await _storage.FetchIndexerAndExplorer(false);

                        await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

            }
        }
    }
}
