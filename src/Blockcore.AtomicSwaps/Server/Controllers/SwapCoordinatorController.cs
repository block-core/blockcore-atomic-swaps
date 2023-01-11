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
	        return await _storageService.Get();
        }

        [HttpGet]
        [Route("session/{swapSessionId}")]
        public async Task<SwapSession?> GetBySession(string swapSessionId)
        {
	        return await _storageService.Get(swapSessionId);
        }

        [HttpPost]
        [Route("create")]
        public async Task CreateSession(CreateSwapSession data)
        {
            SwapSession session = new()
            {
                SwapSessionId = data.SwapSessionId,
                Created = DateTime.UtcNow,
                Status = SwapsDataStatus.Available,
                SharedSecretHash = data.SharedSecretHash,
                CoinSeller = new SwapSessionCoin { CoinSymbol = data.FromCoinSymbol, Amount = data.AmountToSell, SenderPubkey = data.SenderPubkey },
                CoinBuyer = new SwapSessionCoin { CoinSymbol = data.ToCoinSymbol, Amount = data.AmountToBuy, ReceiverPubkey = data.SenderPubkey }
            };

            await _storageService.Add(session);
        }
        
        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateSession(SwapSession data)
        {
	        var swapSession = await this.GetBySession(data.SwapSessionId);

	        if (swapSession == null)
	        {
		        return NotFound();
	        }

	        if (data.CoinSeller.SenderPubkey != swapSession.CoinSeller.SenderPubkey)
		        throw new Exception("Invalid CoinSeller owner");

	        if (swapSession.CoinBuyer.SenderPubkey != null && data.CoinBuyer.SenderPubkey != swapSession.CoinBuyer.SenderPubkey)
		        throw new Exception("Invalid CoinBuyer owner");

	        await _storageService.Update(data);

	        return Ok();
        }

        [HttpDelete]
        [Route("delete/{swapSessionId}")]
        public async Task DeleteSession(string swapSessionId)
        {
	        var swap = await _storageService.Get(swapSessionId);
	        if (swap != null)
	        {
		        await _storageService.Complete(swap);
	        }
        }
	}
}