namespace OnlyT.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyT.Utils;

    [TestClass]
    public class TestDateCalcs
    {
        [TestMethod]
        public void TestMondayOfWeekCalc()
        {
            var monday = new DateTime(2018, 2, 12);
            {
                var dt = DateUtils.GetMondayOfWeek(monday);
                Assert.AreEqual(monday, dt);
            }

            {
                var dt = DateUtils.GetMondayOfWeek(monday.AddDays(-1));
                Assert.AreEqual(dt, monday.AddDays(-7));
            }

            {
                var dt = DateUtils.GetMondayOfWeek(monday.AddDays(1));
                Assert.AreEqual(dt, monday);
            }
            
            {
                var dt = DateUtils.GetMondayOfWeek(monday.AddDays(6));
                Assert.AreEqual(dt, monday);
            }

            {
                var dt = DateUtils.GetMondayOfWeek(monday.AddDays(7));
                Assert.AreEqual(dt, monday.AddDays(7));
            }
        }
    }
}
