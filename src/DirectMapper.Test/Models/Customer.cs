using System;

namespace DirectMapper.Test.Models
{
    public partial class Customer
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = "";

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = "";

        public DateTime BirthDate { get; set; }

        public string DateOfFirstPurchase { get; set; } = "";

        public decimal YearlyIncome { get; set; }

        public string AddressLine1 { get; set; } = "";

        public string? AddressLine2 { get; set; }

        public string Phone { get; set; } = "";

        public string Gender { get; set; } = "";

        public string? EmailAddress { get; set; }

        public MaritalStatuses MaritalStatus { get; set; }
    }

    public partial class Customer
    {
        //public override bool Equals(object? obj) => Helper.ArePropertiesEqual(this, obj as Customer);

        //public override int GetHashCode() => Id;

        //public static bool operator ==(Customer p1, Customer p2) => p1.Equals(p2);

        //public static bool operator !=(Customer p1, Customer p2) => !p1.Equals(p2);
    }
}