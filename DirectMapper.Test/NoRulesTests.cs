using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DirectMapper.Test
{
    [TestClass]
    public class NoRulesTests : TestBase
    {
        [TestMethod]
        public void Clone()
        {
            var srcObject = customers[0];
            var destObject = srcObject.DirectMap<Customer, Customer>();
            Assert.IsTrue(Helper.ArePropertiesEqual(srcObject, destObject), "Cloned object");
            Assert.AreNotSame(srcObject, destObject, "Cloned object");

            var destList = customers.DirectMapRange<Customer, Customer>();
            Assert.IsTrue(Helper.AreItemsEqual(customers, destList), "Cloned list");
            Assert.AreNotSame(customers, destList, "Cloned list");
            Assert.AreNotSame(customers[0], destList.First(), "Cloned list");
        }

        [TestMethod]
        public void SimpleCopy()
        {
            var srcObject = customers[0];
            var destObject = srcObject.DirectMap<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.ArePropertiesMatched(destObject, srcObject), "Cloned object");

            var destList = customers.DirectMapRange<Customer, CustomerViewModel>();
            Assert.IsTrue(Helper.AreItemsMatched(customers, destList), "Cloned list");
        }
    }
}