using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class GlobalRulesTests
    {
        private static Customer customer = null!;
        private static Purchase purchase = null!;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            customer = Fake.GetFakeCustomer();
            purchase = Fake.GetFakePurchase();

            DMapper.ResetForNextTest();

            DMapper
                .BuildGlobalRules()
                .WithRule<string, DateTime>(src => DateTime.Parse(src))
                .WithRule<DateTime, string>(src => src.ToString("yyyy/mm/dd"))
                .WithRule<string, Genders>(src => Enum.TryParse(src, out Genders res) ? res : throw new Exception("Invalid Gender"))
                .WithGlobalToString()
                .Build();
        }

        [TestMethod]
        public void GlobalRules()
        {
            var cust = customer;
            var custVM = cust.DirectMap<Customer, CustomerViewModel>();
            var pur = purchase;
            var purVM = pur.DirectMap<Purchase, PurchaseViewModel>();

            // DateTime -> String
            Assert.AreEqual(custVM.BirthDate, cust.BirthDate.ToString("yyyy/mm/dd"), nameof(CustomerViewModel.BirthDate));
            Assert.AreEqual(purVM.PurchaseDate, pur.PurchaseDate.ToString("yyyy/mm/dd"), nameof(PurchaseViewModel.PurchaseDate));

            // String -> DateTime
            Assert.AreEqual(custVM.DateOfFirstPurchase, DateTime.Parse(cust.DateOfFirstPurchase), nameof(CustomerViewModel.DateOfFirstPurchase));

            // String -> Genders
            Assert.AreEqual(Enum.Parse<Genders>(cust.Gender), custVM.Gender, nameof(CustomerViewModel.Gender));

            // Global ToString
            Assert.AreEqual(cust.MaritalStatus.ToString(), custVM.MaritalStatus, nameof (CustomerViewModel.MaritalStatus));
        }
    }
}