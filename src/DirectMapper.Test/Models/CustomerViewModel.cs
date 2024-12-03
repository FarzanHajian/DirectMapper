using System;

namespace DirectMapper.Test.Models
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = "";

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = "";

        public string BirthDate { get; set; } = "";

        public DateTime DateOfFirstPurchase { get; set; }

        public decimal YearlyIncome { get; set; }

        public string AddressLine1 { get; set; } = "";

        public string? AddressLine2 { get; set; }

        public string? Phone { get; set; } = "";

        public Genders Gender { get; set; }

        public string EmailAddress { get; set; } = "";

        public string MaritalStatus { get; set; } = "";
    }
}