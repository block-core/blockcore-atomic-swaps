using Blockcore.AtomicSwaps.Server.Services;
using Blockcore.AtomicSwaps.Shared;
using Blockcore.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.AtomicSwaps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwapCoordinatorController : ControllerBase
    {
        private readonly ILogger<SwapCoordinatorController> _logger;
        private readonly IStorageService _storageService;


        public SwapCoordinatorController(ILogger<SwapCoordinatorController> logger, IStorageService storageService)
        {
	        _logger = logger;
	        _storageService = storageService;
        }

        [HttpGet]
        public async Task<IEnumerable<SwapSession>> Get()
        {
	        var swaps = await _storageService.Get();

	        List<SwapSession> swapList = new();

	        foreach (var swap in swaps)
	        {
		        var swapItem = System.Text.Json.JsonSerializer.Deserialize<SwapSession>(swap.Data);

		        Guard.NotNull(swapItem, nameof(swapItem));

				swapList.Add(swapItem);

			}

	        return swapList;

        }

        [HttpGet]
        [Route("session/{swapSessionId}")]
        public async Task<SwapSession?> GetBySession(string swapSessionId)
        {
			var swaps = await _storageService.Get(swapSessionId);

			List<SwapSession> swapList = new();

			foreach (var swap in swaps)
			{
				var swapItem = System.Text.Json.JsonSerializer.Deserialize<SwapSession>(swap.Data);

				Guard.NotNull(swapItem, nameof(swapItem));

				swapList.Add(swapItem);

			}

			return swapList.FirstOrDefault();

			return null;
        }

        [HttpPost]
        [Route("create")]
        public async Task CreateSession(CreateSwapSession data)
        {
            SwapSession session = new()
            {
                SwapSessionId = data.SwapSessionId,
                Created = DateTime.UtcNow,
                Status = "Available",
                SharedSecretHash = data.SharedSecretHash,
                CoinSeller = new SwapSessionCoin { CoinSymbol = data.FromCoinSymbol, Amount = data.AmountToSell, SenderPubkey = data.SenderPubkey },
                CoinBuyer = new SwapSessionCoin { CoinSymbol = data.ToCoinSymbol, Amount = data.AmountToBuy, ReceiverPubkey = data.SenderPubkey }
            };

            SwapsData swapsData = new SwapsData
            {
	            Session = data.SwapSessionId,
	            Data = System.Text.Json.JsonSerializer.Serialize(session)
            };

            await _storageService.Add(swapsData);
        }


        [HttpPost]
        [Route("update")]
        public async Task UpdateSession(SwapSession data)
        {
	        var swapSession = await this.GetBySession(data.SwapSessionId);

	        if (data.CoinSeller.SenderPubkey != swapSession.CoinSeller.SenderPubkey)
		        throw new Exception("Invalid CoinSeller owner");

	        if (swapSession.CoinBuyer.SenderPubkey != null && data.CoinBuyer.SenderPubkey != swapSession.CoinBuyer.SenderPubkey)
		        throw new Exception("Invalid CoinBuyer owner");

	        SwapsData swapsData = new SwapsData
	        {
		        Session = data.SwapSessionId,
		        Data = System.Text.Json.JsonSerializer.Serialize(data)
	        };

	        await _storageService.Add(swapsData);
        }

        [HttpDelete]
        [Route("delete/{swapSessionId}")]
        public async Task DeleteSession(string swapSessionId)
        {
	        var swaps = await _storageService.Get(swapSessionId);

	        var swapSession = swaps.FirstOrDefault();

			await _storageService.Complete(swapSession);
		}
	}
}