using DirectMapper.Test.Models;
using System;
using System.Collections.Generic;

namespace DirectMapper.Test
{
    internal static class Fake
    {
        public static Customer GetFakeCustomer()
        {
            return new Customer
            {
                Id = 100,
                FirstName = "Martin",
                MiddleName = null,
                LastName = "Sommer",
                AddressLine1 = "Sierras de Granada 9993",
                AddressLine2 = "Entrace 3",
                BirthDate = new DateTime(1982, 10, 12),
                DateOfFirstPurchase = "2022/02/24",
                EmailAddress = "customer@customer.com",
                Phone = "(91) 555 22 82",
                Gender = "Male",
                MaritalStatus = MaritalStatuses.Married,
                YearlyIncome = 100000
            };
        }

        public static IEnumerable<Customer> GetFakeCustomers()
        {
            return new Customer[]
            {
                new()
                {
                    Id = 100,
                    FirstName = "Martin",
                    MiddleName = null,
                    LastName = "Sommer",
                    AddressLine1 = "Sierras de Granada 9993",
                    AddressLine2 = "Entrace 3",
                    BirthDate = new DateTime(1982, 10, 12),
                    DateOfFirstPurchase = "2022/02/24",
                    EmailAddress = "martin@customer.com",
                    Phone = "(91) 555 22 82",
                    Gender = "Male",
                    MaritalStatus = MaritalStatuses.Married,
                    YearlyIncome = 100000
                },
                new() {
                    Id = 101,
                    FirstName = "Ann",
                    MiddleName = null,
                    LastName = "Devon",
                    AddressLine1 = "35 King George",
                    AddressLine2 = null,
                    BirthDate = new DateTime(1992, 11, 2),
                    DateOfFirstPurchase = "2023/12/24",
                    EmailAddress = "ann@customer.com",
                    Phone = "(171) 555-0297",
                    Gender = "Female",
                    MaritalStatus = MaritalStatuses.Married,
                    YearlyIncome = 110000
                },
                new() {
                    Id = 102,
                    FirstName = "Giovanni",
                    MiddleName = "Fran",
                    LastName = " Rovelli",
                    AddressLine1 = "Via Ludovico il Moro 22",
                    AddressLine2 = "",
                    BirthDate = new DateTime(1972, 1, 03),
                    DateOfFirstPurchase = "2020/12/14",
                    EmailAddress = "giovanni@customer.com",
                    Phone = "035-640230",
                    Gender = "Male",
                    MaritalStatus = MaritalStatuses.Single,
                    YearlyIncome = 120000
                },
                new() {
                    Id = 103,
                    FirstName = "Paula",
                    MiddleName = "",
                    LastName = "Wilson",
                    AddressLine1 = "2817 Milton Dr.",
                    AddressLine2 = "No. 32",
                    BirthDate = new DateTime(1995, 07, 12),
                    DateOfFirstPurchase = "2021/08/29",
                    EmailAddress = "paula@customer.com",
                    Phone = "(505) 555-5939",
                    Gender = "Femail",
                    MaritalStatus = MaritalStatuses.Divorced,
                    YearlyIncome = 95000
                }
            };
        }

        public static Purchase GetFakePurchase()
        {
            return new Purchase
            {
                Id = 1000,
                CustomerId = 100,
                PurchaseDate = new DateTime(2024, 11, 17),
                Amount = 15000,
                Currency = "USD"
            };
        }
    }
}