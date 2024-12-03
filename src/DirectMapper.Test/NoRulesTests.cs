using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class NoRulesTests
    {
        private static Customer customer = null!;
        private static IEnumerable<Customer> customers = null!;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            customer = Fake.GetFakeCustomer();
            customers = Fake.GetFakeCustomers();

            DMapper.ResetForNextTest();
        }

        [TestMethod]
        public void Clone()
        {
            var srcObj = customer;
            var destObj = srcObj.DirectMap<Customer, Customer>();
            Assert.IsTrue(Helper.ArePropertiesEqual(srcObj, destObj), "Cloned object");
            Assert.AreNotSame(srcObj, destObj, "Cloned object");

            var destList = customers.DirectMapRange<Customer, Customer>();
            Assert.IsTrue(Helper.AreItemsEqual(customers, destList), "Cloned list");
            Assert.AreNotSame(customers, destList, "Cloned list");
            Assert.AreNotSame(customers.First(), destList.First(), "Cloned list");
        }

        [TestMethod]
        public void SimpleCopy()
        {
            var srcObj = customer;
            var destObj = srcObj.DirectMap<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.AreCommonPropertiesEqual(destObj, srcObj), "Cloned object");

            var destList = customers.DirectMapRange<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.AreItemsMatched(customers, destList), "Cloned list");
        }
    }
}