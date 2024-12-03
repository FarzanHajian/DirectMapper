using DirectMapper.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static System.Collections.Specialized.BitVector32;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class ErrorTests
    {
        private static Customer customer = null!;
        private static Purchase purchase = null!;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            customer = Fake.GetFakeCustomer();
            purchase = Fake.GetFakePurchase();
        }

        [TestMethod]
        public void InvalidRuleReturnType()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildMapper<Customer, CustomerViewModel>()
                    .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                    .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                    .WithRule<string, string>(nameof(Customer.Gender), src => src.ToUpper())                        // Throws exception
                    .WithRule<MaritalStatuses, string>(nameof(Customer.MaritalStatus), src => Enum.GetName(src)!)
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void NoRuleFound()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                // Gender does not have any rule
                DMapper
                    .BuildMapper<Customer, CustomerViewModel>()
                    .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                    .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                    .WithRule<MaritalStatuses, string>(nameof(Customer.MaritalStatus), src => Enum.GetName(src)!)
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void DuplicateSpecificRule()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildMapper<Customer, CustomerViewModel>()
                    .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                    .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                    .WithRule<string, Genders>(nameof(Customer.Gender), src => { Enum.TryParse(src, out Genders dest); return dest; })
                    .WithRule<MaritalStatuses, string>(nameof(Customer.MaritalStatus), src => Enum.GetName(src)!)
                    .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy-MM-dd"))      // Duplicate
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void DuplicateSpecificRuleSet()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
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

                // The rules to map from Purchase to PurchaseViewModel are already defined.
                DMapper
                    .BuildMapper<Purchase, PurchaseViewModel>()
                    .WithRule<decimal, string>(nameof(Purchase.Amount), src => src.ToString("N3"))
                    .WithRule<DateTime, string>(nameof(Purchase.PurchaseDate), src => src.ToString("yyyy-MM-dd"))
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void DuplicateGlobalRule()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildGlobalRules()
                    .WithRule<DateTime, string>(src=>src.ToString("yyyy/MM/dd"))
                    .WithRule<MaritalStatuses, string>(src => Enum.GetName(src)!)
                    .WithRule<DateTime, string>(src => src.ToString("yyyy-MM-dd"))  // Duplicate
                    .WithGlobalToString()
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void DuplicateGlobalToStringRule()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildGlobalRules()
                    .WithRule<DateTime, string>(src => src.ToString("yyyy/MM/dd"))
                    .WithGlobalToString()
                    .WithRule<MaritalStatuses, string>(src => Enum.GetName(src)!)
                    .WithGlobalToString()
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void DuplicateGlobalRuleSet()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildGlobalRules()
                    .WithRule<DateTime, string>(src => src.ToString("yyyy/MM/dd"))
                    .WithRule<MaritalStatuses, string>(src => Enum.GetName(src)!)
                    .WithGlobalToString()
                    .Build();

                // Duplicate global rules
                DMapper
                    .BuildGlobalRules()
                    .WithRule<MaritalStatuses, string>(src => Enum.GetName(src)!)
                    .WithRule<DateTime, string>(src => src.ToString("yyyy-MM-dd"))
                    .WithGlobalToString()
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GlobalRulesAfterSpecificRules()
        {
            DMapper.ResetForNextTest();

            Action action = () =>
            {
                DMapper
                    .BuildMapper<Purchase, PurchaseViewModel>()
                    .WithRule<DateTime, string>(nameof(Purchase.PurchaseDate), src => src.ToString("yyyy/MM/dd"))
                    .WithRule<decimal, string>(nameof(Purchase.Amount), src => src.ToString("N2"))
                    .Build();

                DMapper
                    .BuildGlobalRules()
                    .WithRule<MaritalStatuses, string>(src => Enum.GetName(src)!)
                    .WithRule<DateTime, string>(src => src.ToString("yyyy-MM-dd"))
                    .WithGlobalToString()
                    .Build();
            };

            Assert.ThrowsException<InvalidOperationException>(action);
        }
    }
}