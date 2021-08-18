using System;

namespace DirectMapper.Test.Models
{
    internal class CustomerViewModel
    {
        public DateTime BirthDate { get; set; }

        public DateTime DateFirstPurchase { get; set; }

        public short TotalChildren { get; set; }

        public short NumberChildrenAtHome { get; set; }

        public int CustomerKey { get; set; }

        public decimal YearlyIncome { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Phone { get; set; }

        public string CommuteDistance { get; set; }

        public string Gender { get; set; }

        public string EmailAddress { get; set; }

        public string EnglishOccupation { get; set; }

        public string MaritalStatus { get; set; }
    }
}