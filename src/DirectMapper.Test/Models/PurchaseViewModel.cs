namespace DirectMapper.Test.Models
{
    public class PurchaseViewModel
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string PurchaseDate { get; set; } = "";

        public string Amount { get; set; } = "";

        public string Currency { get; set; } = "";
    }
}