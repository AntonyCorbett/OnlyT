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

        [TestMethod]
        public void TestNearest15Mins()
        {
            DateTime tenAm = DateTime.Today + TimeSpan.FromHours(10);
            DateTime tenFifteenAm = DateTime.Today + TimeSpan.FromHours(10) + TimeSpan.FromMinutes(15);

            var result = DateUtils.GetNearestQuarterOfAnHour(tenAm);
            Assert.AreEqual(result, tenAm);

            var dt = tenAm.AddMinutes(5);
            result = DateUtils.GetNearestQuarterOfAnHour(dt);
            Assert.AreEqual(result, tenAm);

            dt = tenAm.AddMinutes(10);
            result = DateUtils.GetNearestQuarterOfAnHour(dt);
            Assert.AreEqual(result, tenAm);

            dt = tenAm.AddMinutes(11);
            result = DateUtils.GetNearestQuarterOfAnHour(dt);
            Assert.AreEqual(result, tenFifteenAm);
        }
    }
}
