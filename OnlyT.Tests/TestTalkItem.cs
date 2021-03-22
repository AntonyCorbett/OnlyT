namespace OnlyT.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyT.Models;

    [TestClass]
    public class TestTalkItem
    {
        [TestMethod]
        public void TestOriginalBell()
        {
            var item = new TalkScheduleItem(10000, "A Name", string.Empty, string.Empty)
            {
                AutoBell = true
            };

            Assert.IsTrue(item.AutoBell);
            Assert.IsTrue(item.OriginalAutoBell);

            item.AutoBell = false;
            Assert.IsFalse(item.AutoBell);
            Assert.IsTrue(item.OriginalAutoBell);
        }

        [TestMethod]
        public void TestOriginalDuration()
        {
            var testDur = TimeSpan.FromMinutes(10);
            var changedDur = TimeSpan.FromMinutes(20);

            var item = new TalkScheduleItem(10000, "A Name", string.Empty, string.Empty)
            {
                OriginalDuration = testDur
            };
            Assert.AreEqual(item.OriginalDuration, testDur);
            Assert.AreEqual(item.ActualDuration, testDur);
            
            Assert.IsNull(item.ModifiedDuration);
            Assert.IsNull(item.AdaptedDuration);
            
            Assert.IsTrue(item.GetPlannedDurationSeconds() == (int)testDur.TotalSeconds);

            item.ModifiedDuration = changedDur;
            Assert.AreEqual(item.OriginalDuration, testDur);
            Assert.AreEqual(item.ModifiedDuration, changedDur);
            
            Assert.IsTrue(item.GetPlannedDurationSeconds() == (int)changedDur.TotalSeconds);
        }
    }
}
