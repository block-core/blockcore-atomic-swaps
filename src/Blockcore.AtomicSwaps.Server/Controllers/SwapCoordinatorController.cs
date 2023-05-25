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
        [HttpPost]
        public async Task<IEnumerable<SwapSession>> Post(List<string> pubKeys)
        {
            return await _storageService.Get(pubKeys);
        }
        [HttpGet]
        [Route("session/{swapSessionId}")]
        public async Task<SwapSession?> GetBySession(string swapSessionId)
        {
            return await _storageService.Get(swapSessionId);
        }

        [HttpPost]
        [Route("create")]
        public async Task CreateSession(SwapSession data)
        {
            await _storageService.Add(data);
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

            if (data.SwapMaker.SenderPubkey != swapSession.SwapMaker.SenderPubkey)
                throw new Exception("Invalid SwapMaker owner");

            if (swapSession.SwapTaker.SenderPubkey != null && data.SwapTaker.SenderPubkey != swapSession.SwapTaker.SenderPubkey)
                throw new Exception("Invalid SwapTaker owner");

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