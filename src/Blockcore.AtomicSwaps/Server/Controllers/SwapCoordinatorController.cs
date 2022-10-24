using Blockcore.AtomicSwaps.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.AtomicSwaps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwapCoordinatorController : ControllerBase
    {
        private readonly ILogger<SwapCoordinatorController> _logger;


        public static Dictionary<string, SwapSession> Swaps = new Dictionary<string, SwapSession>();

        public SwapCoordinatorController(ILogger<SwapCoordinatorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<SwapSession> Get()
        {
            return Swaps.Values.ToArray();
        }

        [HttpGet]
        [Route("session/{swapSessionId}")]
        public SwapSession? GetBySession(string swapSessionId)
        {
            if (Swaps.TryGetValue(swapSessionId, out SwapSession? swapSession))
            {
                return swapSession;
            }

            return null;
        }

        [HttpPost]
        [Route("create")]
        public void CreateSession(CreateSwapSession data)
        {
            SwapSession session = new()
            {
                SwapSessionId = data.SwapSessionId,
                Created = DateTime.UtcNow,
                Status = "Available",
                CoinSeller = new SwapSessionCoin {CoinSymbol = data.FromCoinSymbol, Amount = data.AmountToSell, OwnerPubkey = data.OwnerPubkey},
                CoinBuyer= new SwapSessionCoin { CoinSymbol = data.ToCoinSymbol, Amount = data.AmountToBuy}
            };

            Swaps.TryAdd(session.SwapSessionId, session);

        }


        [HttpPost]
        [Route("update")]
        public void UpdateSession(SwapSession data)
        {
            if (Swaps.TryGetValue(data.SwapSessionId, out SwapSession swapSession))
            {
                Swaps[data.SwapSessionId] = data;
            }
        }
    }
}