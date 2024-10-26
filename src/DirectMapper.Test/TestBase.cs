using DirectMapper.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DirectMapper.Test
{
    [TestClass]
    public abstract class TestBase
    {
        private protected List<Customer> customers = null;

        [TestInitialize]
        public void Intialize()
        {
            customers = Helper.LoadCustomers();
        }
    }
}