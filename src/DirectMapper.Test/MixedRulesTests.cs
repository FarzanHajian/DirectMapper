using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class MixedRulesTests
    {
        private static Customer customer = null!;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            customer = Fake.GetFakeCustomer();

            DMapper.ResetForNextTest();

            DMapper
                .BuildGlobalRules()
                .WithRule<string, Genders>(src => { Enum.TryParse(src, out Genders dest); return dest; })
                .WithGlobalToString()
                .Build();

            DMapper
                .BuildMapper<Customer, CustomerViewModel>()
                .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                .Build();
        }

        [TestMethod]
        public void MixedRules()
        {
            string[] customerRuledProps = new[] { nameof(Customer.DateOfFirstPurchase), nameof(Customer.BirthDate), nameof(Customer.Gender), nameof(Customer.MaritalStatus) };

            var custVM = customer.DirectMap<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(customer, custVM, customerRuledProps));   // Properties without rule
            Assert.AreEqual(DateTime.Parse(customer.DateOfFirstPurchase), custVM.DateOfFirstPurchase);
            Assert.AreEqual(customer.BirthDate.ToString("yyyy/MM/dd"), custVM.BirthDate);
            Enum.TryParse(customer.Gender, out Genders gender);
            Assert.AreEqual(gender, custVM.Gender);                                     // Global rule
            Assert.AreEqual(customer.MaritalStatus.ToString(), custVM.MaritalStatus);   // Global ToString rule
        }
    }
}