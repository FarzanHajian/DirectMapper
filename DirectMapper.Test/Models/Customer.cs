using System;

namespace DirectMapper.Test.Models
{
    internal class Customer
    {
        public string BirthDate { get; set; }

        public string DateFirstPurchase { get; set; }

        public short TotalChildren { get; set; }

        public short NumberChildrenAtHome { get; set; }

        public short NumberCarsOwned { get; set; }

        public int CustomerKey { get; set; }

        public int GeographyKey { get; set; }

        public decimal YearlyIncome { get; set; }

        public bool NameStyle { get; set; }

        public string CustomerAlternateKey { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Suffix { get; set; }

        public string SpanishOccupation { get; set; }

        public string FrenchOccupation { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Phone { get; set; }

        public string CommuteDistance { get; set; }

        public string Gender { get; set; }

        public string EmailAddress { get; set; }

        public string EnglishEducation { get; set; }

        public string SpanishEducation { get; set; }

        public string FrenchEducation { get; set; }

        public string EnglishOccupation { get; set; }

        public string MaritalStatus { get; set; }

        public string HouseOwnerFlag { get; set; }

        public override bool Equals(object obj) => Helper.ArePropertiesEqual(this, (Customer)obj);

        public static bool operator ==(Customer p1, Customer p2) => p1.Equals(p2);

        public static bool operator !=(Customer p1, Customer p2) => !p1.Equals(p2);
    }
}