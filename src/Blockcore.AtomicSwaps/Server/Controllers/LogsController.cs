using System;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.Server.Services;
using Blockcore.AtomicSwaps.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Blockcore.AtomicSwaps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        private readonly ITelegramBotService _telegramBotService;

        public LogsController(ILogger<LogsController> logger, ITelegramBotService telegramBotService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telegramBotService = telegramBotService;
        }

        [HttpPost]
        public async Task<IActionResult> PostLog(ClientLog log)
        {
            // TODO: Save the client's `log` in the database

            _logger.Log(log.LogLevel, log.EventId, log.Url + Environment.NewLine + log.Message);

            await _telegramBotService.SendLogAsync(log);

            return Ok();
        }
    }
}