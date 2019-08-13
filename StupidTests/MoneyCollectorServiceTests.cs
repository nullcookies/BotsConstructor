using DataLayer;
using DataLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Website.Services;

namespace StupidTests
{
    [TestClass]
    public class MoneyCollectorServiceTests
    {
     
        [TestMethod]
        public void GetTodayDateTest()
        {
            var date = MoneyCollectorService.GetTodayDate();

            Assert.IsTrue(date.Hour   ==  0);
            Assert.IsTrue(date.Minute ==  0);
            Assert.IsTrue(date.Second ==  0);
            
        }
    }
}
