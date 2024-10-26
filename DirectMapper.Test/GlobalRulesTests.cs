using DirectMapper.Test.Models;
using FarzanHajian.DirectMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

namespace DirectMapper.Test
{
    [TestClass]
    public class GlobalRulesTests : TestBase
    {
        [TestMethod]
        public void GlobalStringToDateTimeRule()
        {
            var srcObject = customers[0];
            var destObject = srcObject.DirectMap<Customer, CustomerViewModel>();
            Assert.AreEqual(destObject.BirthDate, DateTime.Parse(srcObject.BirthDate), nameof(CustomerViewModel.BirthDate));
            Assert.AreEqual(destObject.DateFirstPurchase, DateTime.Parse(srcObject.DateFirstPurchase), nameof(CustomerViewModel.DateFirstPurchase));
        }

        [TestInitialize]
        public void Initiaize()
        {
            DMapper
               .BuildGlobalRules()
               .WithRule<string, DateTime>((src) => DateTime.Parse(src))
               .Build();
        }
    }
}