namespace AKAOFinance
{
    public class Purchase
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public Purchase(string Name, decimal Price, string Descrition = "")
        {
            this.Name = Name;
            this.Price = Price;
            this.Description = Descrition;
        }
    }
}
