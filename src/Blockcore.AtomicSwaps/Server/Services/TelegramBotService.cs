using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Blockcore.AtomicSwaps.Server.Services
{
    public class TelegramLoggingBotOptions
    {
        public string AccessToken { get; set; }
        public string ChatId { get; set; }
    }

    public interface ITelegramBotService
    {
        Task SendLogAsync(ClientLog log);
    }

    public class TelegramBotService : ITelegramBotService
    {
        private readonly string _chatId;
        private readonly TelegramBotClient _client;

        public TelegramBotService(IOptions<TelegramLoggingBotOptions> options)
        {
            _chatId = options.Value.ChatId;
            _client = new TelegramBotClient(options.Value.AccessToken);
        }

        public async Task SendLogAsync(ClientLog log)
        {
            var text = formatMessage(log);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            await _client.SendTextMessageAsync(chatId: _chatId, text: text);
           // await _client.SendTextMessageAsync(chatId: _chatId, text: text , parseMode:ParseMode.MarkdownV2);
        }

        private static string formatMessage(ClientLog log)
        {
            if (string.IsNullOrWhiteSpace(log.Message))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(toEmoji(log.LogLevel))
                .Append(" *")
                .AppendFormat("{0:hh:mm:ss}", DateTime.Now)
                .Append("* ")
                .AppendLine(log.Message);

            if (!string.IsNullOrWhiteSpace(log.Exception))
            {
                sb.AppendLine()
                    .Append('`')
                    .AppendLine(log.Exception)
                    .AppendLine(log.StackTrace)
                    .AppendLine("`")
                    .AppendLine();
            }

            sb.Append("*Url:* ").AppendLine(log.Url);
            return sb.ToString();
        }

        private static string toEmoji(LogLevel level) =>
            level switch
            {
                LogLevel.Trace => "⬜️",
                LogLevel.Debug => "🟦",
                LogLevel.Information => "⬛️️️",
                LogLevel.Warning => "🟧",
                LogLevel.Error => "🟥",
                LogLevel.Critical => "❌",
                LogLevel.None => "🔳",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
    }
}