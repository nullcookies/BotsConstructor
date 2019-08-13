using DataLayer;
using DataLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StupidTests
{
    [TestClass]
    public class MyTests
    {
        [TestMethod]
        public void TestMethod()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5, 6, 6, 5, 5, 5, 4, 8, 5, 2, 15, 1, 5, 6, 1 };

            list = list.GroupBy(_val => _val).Select(_val => _val.First()).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var val = list[i];
                for (int j = i+1; j<list.Count ; j++)
                {
                    if (val == list[j])
                    {
                        Assert.Fail();
                    }
                }
            }
            

        }
    }
}
