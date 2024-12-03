using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class OtherTests
    {
        private static Customer customer = null!;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            customer = Fake.GetFakeCustomer();

            DMapper.ResetForNextTest();

            DMapper
                .BuildMapper<Customer, CustomerViewModel>()
                .WithRule<string, DateTime>(nameof(Customer.DateOfFirstPurchase), src => DateTime.Parse(src))
                .WithRule<DateTime, string>(nameof(Customer.BirthDate), src => src.ToString("yyyy/MM/dd"))
                .WithRule<string, Genders>(nameof(Customer.Gender), src => { Enum.TryParse(src, out Genders dest); return dest; })
                .WithRule<MaritalStatuses, string>(nameof(Customer.MaritalStatus), src => Enum.GetName(src)!)
                .Build();
        }

        [TestMethod]
        public void NullSource()
        {
            Customer? customer = null;
            CustomerViewModel? custVM = customer!.DirectMap<Customer, CustomerViewModel>();
            Assert.IsNull(custVM);

            IEnumerable<Customer>? customers = null;
            IEnumerable<CustomerViewModel>? custVMs = customers.DirectMapRange<Customer, CustomerViewModel>();
            Assert.IsNull(custVMs);
        }

        [TestMethod]
        public void Mapper()
        {
            Func<Customer, CustomerViewModel> mapper = DMapper.GetMapper<Customer, CustomerViewModel>();
            CustomerViewModel custVM1 = mapper(customer);
            CustomerViewModel custVM2 = customer.DirectMap<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.ArePropertiesEqual(custVM1, custVM2));
        }
    }
}