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
            TalkScheduleItem item = new TalkScheduleItem { Bell = true };
            Assert.IsTrue(item.Bell);
            Assert.IsTrue(item.OriginalBell);

            item.Bell = false;
            Assert.IsFalse(item.Bell);
            Assert.IsTrue(item.OriginalBell);
        }

        [TestMethod]
        public void TestOriginalDuration()
        {
            TimeSpan testDur = TimeSpan.FromMinutes(10);
            TimeSpan changedDur = TimeSpan.FromMinutes(20);

            TalkScheduleItem item = new TalkScheduleItem { OriginalDuration = testDur };
            Assert.AreEqual(item.OriginalDuration, testDur);
            Assert.AreEqual(item.ActualDuration, testDur);
            
            Assert.IsNull(item.ModifiedDuration);
            Assert.IsNull(item.AdaptedDuration);
            
            Assert.IsTrue(item.GetDurationSeconds() == (int)testDur.TotalSeconds);

            item.ModifiedDuration = changedDur;
            Assert.AreEqual(item.OriginalDuration, testDur);
            Assert.AreEqual(item.ModifiedDuration, changedDur);
            
            Assert.IsTrue(item.GetDurationSeconds() == (int)changedDur.TotalSeconds);
        }
    }
}
