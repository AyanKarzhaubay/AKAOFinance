namespace AKAOFinance
{
    public class User
    {
        public long ChatId { get; set; }
        public string Name { get; set; }

        public User(long chatId, string name)
        {
            this.ChatId = chatId;
            this.Name = name;
        }
    }
}
