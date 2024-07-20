using Azure;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AKAOFinance
{
    public class MessageHandler
    {
        private readonly DatabaseHelper dbHelper;
        private readonly ILogger logger;
        public MessageHandler(DatabaseHelper dbHelper, ILogger logger)
        {
            this.dbHelper = dbHelper;
            this.logger = logger;
        }
        public async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandleMessageAsync(botClient, update.Message);
                return;
            }
        }
        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message msg)
        {
            logger.LogInformation($"Received a message with the text: {msg.Text}");
            string warningMsg = "Введите команду /help чтобы узнать доступные команды для бота.";

            if (msg.Text == "/start")
            {
                bool isExisted = await dbHelper.IsChatIdExistedAsync(msg.Chat.Id);
                if (!isExisted)
                {
                    User user = new User(msg.Chat.Id, $"{msg.Chat.FirstName} {msg.Chat.LastName}");
                    await dbHelper.AddChatAsync(user);
                }
                await botClient.SendTextMessageAsync(msg.Chat.Id, warningMsg);
                return;
            }
            if (msg.Text == "/mypurchases")
            {
                await HandleMyPurchasesAsync(botClient, msg.Chat.Id);
                return;
            }
            if (msg.Text.StartsWith("/help"))
            {
                await HandleHelpAsync(botClient, msg.Chat.Id);
                return;
            }
            string[] data = msg.Text.Split('.');
            if (data.Length != 2 && data.Length != 3)
            {
                await botClient.SendTextMessageAsync(msg.Chat.Id, warningMsg);
                return;
            }

            var purchase = new Purchase(data[0], Convert.ToDecimal(data[1]), data.Length == 3 ? data[2] : string.Empty);

            await dbHelper.AddPurchaseAsync(purchase, msg.Chat.Id);
        }

        private async Task HandleHelpAsync(ITelegramBotClient botClient, long chatId)
        {
            string help = $"/start - начало работы с ботом" +
                $"\n/help - список команд для бота" +
                $"\n/mypurchases - список ваших покупок" +
                $"\nЧтобы ввести вашу покупку, нужно ввести данные в следующем формате: " +
                $"\nИмя покупки.Цена.Описание.\n\nОписание опционально.";
            await botClient.SendTextMessageAsync(chatId, help);
        }

        public Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Ошибка телеграм API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
        private async Task HandleMyPurchasesAsync(ITelegramBotClient botClient, long chatId)
        {
            var purchases = await dbHelper.GetPurchasesByChatIdAsync(chatId);
            if (purchases == null || !purchases.Any())
            {
                await botClient.SendTextMessageAsync(chatId, "У вас нет сохраненных покупок.");
                return;
            }

            var response = "Ваши покупки:\n";
            foreach (var purchase in purchases)
            {
                response += $"- {purchase.Name}: {purchase.Price}\n";
                if (!string.IsNullOrEmpty(purchase.Description))
                {
                    response += $"  Описание: {purchase.Description}\n";
                }
            }

            await botClient.SendTextMessageAsync(chatId, response);
        }
    }
}
