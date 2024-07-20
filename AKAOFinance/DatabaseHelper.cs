using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AKAOFinance
{
    public class DatabaseHelper
    {
        private readonly string connectionString;
        private readonly ILogger logger;
        public DatabaseHelper(string connectionString, ILogger logger)
        {
            this.connectionString = connectionString;
            this.logger = logger;
        }

        public async Task<bool> IsChatIdExistedAsync(long chatId)
        {
            using (SqlConnection sql = new SqlConnection(connectionString))
            {
                await sql.OpenAsync();
                using (SqlCommand command = new SqlCommand("select dbo.IsThisChatIdExisted(@ChatId)", sql))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    object? result = await command.ExecuteScalarAsync();
                    logger.LogInformation($"Checked if chat ID {chatId} exists: {result}");
                    return result is bool isExisted && isExisted;
                }
            }
        }

        public async Task AddChatAsync(User user)
        {
            using (SqlConnection sql = new SqlConnection(connectionString))
            {
                await sql.OpenAsync();
                using (SqlCommand command = new SqlCommand(
                    "insert into Users (ChatID, Name, DateTime) values (@ChatId, @Name, @DateTime)", sql))
                {
                    command.Parameters.AddWithValue("@ChatId", user.ChatId);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@DateTime", DateTime.Now);
                    await command.ExecuteNonQueryAsync();
                    logger.LogInformation($"Added chat with ID {user.ChatId} and name {user.Name} to database.");
                }
            }
        }

        public async Task AddPurchaseAsync(Purchase purchase, long chatId)
        {
            using (SqlConnection sql = new SqlConnection(connectionString))
            {
                await sql.OpenAsync();
                using (SqlCommand command = new SqlCommand(
                    "insert into Purchases (Name, Price, Description, DateTime, ChatID) values (@Name, @Price, @Description, @DateTime, @ChatId)", sql))
                {
                    command.Parameters.AddWithValue("@Name", purchase.Name);
                    command.Parameters.AddWithValue("@Price", purchase.Price);
                    command.Parameters.AddWithValue("@Description", purchase.Description);
                    command.Parameters.AddWithValue("@DateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    await command.ExecuteNonQueryAsync();
                    logger.LogInformation($"Added purchase {purchase.Name} with price {purchase.Price} and description {purchase.Description} for chat ID {chatId} to database.");
                }
            }
        }
        public async Task<List<Purchase>> GetPurchasesByChatIdAsync(long chatId)
        {
            using (SqlConnection sql = new SqlConnection(connectionString))
            {
                await sql.OpenAsync();
                using (SqlCommand command = new SqlCommand(
                    "select Name, Price, Description, DateTime from Purchases where ChatID = @ChatId", sql))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var purchases = new List<Purchase>();
                        while (await reader.ReadAsync())
                        {
                            purchases.Add(new Purchase(reader["Name"].ToString(), (decimal)reader["Price"], reader["Description"].ToString()));
                        }
                        return purchases;
                    }
                }
            }
        }
    }
}