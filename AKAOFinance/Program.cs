using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace AKAOFinance
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            string connectionString = @"Data Source=KARZHAUBAYAYAN;Initial Catalog=OurFinance;Integrated Security=True;TrustServerCertificate=True";
            string token = "Your token here";
            Console.Title = "TelegramBot";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            TelegramBotClient botClient = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();
            DatabaseHelper dbHelper = new DatabaseHelper(connectionString, logger);
            MessageHandler handler = new MessageHandler(dbHelper, logger);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                handler.HandleUpdatesAsync,
                handler.HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync();

            logger.LogInformation($"Запущен бот @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
