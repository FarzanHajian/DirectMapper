using System;

namespace DirectMapper.Test.Models
{
    public partial class Purchase
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "";
    }

    public partial class Purchase
    {
        //public override bool Equals(object? obj) => Helper.ArePropertiesEqual(this, obj as Purchase);

        //public override int GetHashCode() => Id;

        //public static bool operator ==(Purchase p1, Purchase p2) => p1.Equals(p2);

        //public static bool operator !=(Purchase p1, Purchase p2) => !p1.Equals(p2);
    }
}