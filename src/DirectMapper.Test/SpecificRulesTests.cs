using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class SpecificRulesTests
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
                .BuildMapper<Customer, CustomerViewModel>()
                .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                .WithRule<string, Genders>(nameof(Customer.Gender), src => { Enum.TryParse(src, out Genders dest); return dest; })
                .WithRule<MaritalStatuses, string>(nameof(Customer.MaritalStatus), src => Enum.GetName(src)!)
                .Build();

            DMapper
                .BuildMapper<CustomerViewModel, Customer>()
                .WithRule<DateTime, string>(nameof(CustomerViewModel.DateOfFirstPurchase), src => src.ToString("yyyy-MM-dd"))
                .WithRule<string, DateTime>(nameof(CustomerViewModel.BirthDate), src => DateTime.Parse(src))
                .WithRule<Genders, string>(nameof(CustomerViewModel.Gender), src => Enum.GetName(src)!.ToUpper())
                .WithRule<string, MaritalStatuses>(nameof(CustomerViewModel.MaritalStatus), src => { Enum.TryParse(src, out MaritalStatuses dest); return dest; })
                .Build();

            DMapper
                .BuildMapper<Purchase, PurchaseViewModel>()
                .WithRule<DateTime, string>(nameof(Purchase.PurchaseDate), src => src.ToString("yyyy/MM/dd"))
                .WithRule<decimal, string>(nameof(Purchase.Amount), src => src.ToString("N2"))
                .Build();

            DMapper
                .BuildMapper<PurchaseViewModel, Purchase>()
                .WithRule<string, DateTime>(nameof(PurchaseViewModel.PurchaseDate), src => DateTime.Parse(src))
                .WithRule<string, decimal>(nameof(Purchase.Amount), src => decimal.TryParse(src, out decimal dest) ? dest : 0)
                .Build();
        }

        [TestMethod]
        public void SpecificRules1()
        {
            string[] customerRuledProps = new[] { nameof(Customer.DateOfFirstPurchase), nameof(Customer.BirthDate), nameof(Customer.Gender), nameof(Customer.MaritalStatus) };

            var custVM = customer.DirectMap<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(customer, custVM, customerRuledProps));   // Properties without rule
            Assert.AreEqual(DateTime.Parse(customer.DateOfFirstPurchase), custVM.DateOfFirstPurchase);
            Assert.AreEqual(customer.BirthDate.ToString("yyyy/MM/dd"), custVM.BirthDate);
            Enum.TryParse(customer.Gender, out Genders gender);
            Assert.AreEqual(gender, custVM.Gender);
            Assert.AreEqual(Enum.GetName(customer.MaritalStatus)!, custVM.MaritalStatus);

            var cust = custVM.DirectMap<CustomerViewModel, Customer>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(custVM, cust, customerRuledProps));
            Assert.AreEqual(custVM.DateOfFirstPurchase.ToString("yyyy-MM-dd"), cust.DateOfFirstPurchase);
            Assert.AreEqual(DateTime.Parse(custVM.BirthDate), cust.BirthDate);
            Assert.AreEqual(Enum.GetName(custVM.Gender)!.ToUpper(), cust.Gender);
            Enum.TryParse(custVM.MaritalStatus, out MaritalStatuses marital);
            Assert.AreEqual(marital, cust.MaritalStatus);
        }

        [TestMethod]
        public void SpecificRules2()
        {
            string[] purchaseRuledProps = new[] { nameof(Purchase.PurchaseDate), nameof(Purchase.Amount) };

            var purVM = purchase.DirectMap<Purchase, PurchaseViewModel>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(purchase, purVM, purchaseRuledProps));
            Assert.AreEqual(purchase.PurchaseDate.ToString("yyyy/MM/dd"), purVM.PurchaseDate);
            Assert.AreEqual(purchase.Amount.ToString("N2"), purVM.Amount);

            var pur = purVM.DirectMap<PurchaseViewModel, Purchase>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(pur, purVM, purchaseRuledProps));
            Assert.AreEqual(DateTime.Parse(purVM.PurchaseDate), pur.PurchaseDate);
            decimal.TryParse(purVM.Amount, out decimal amount);
            Assert.AreEqual(amount, pur.Amount);
        }
    }
}