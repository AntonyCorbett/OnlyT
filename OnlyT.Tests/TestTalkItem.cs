using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyT.Models;

namespace OnlyT.Tests
{
   [TestClass]
   public class TestTalkItem
   {
      [TestMethod]
      public void TestOriginalBell()
      {
         TalkScheduleItem item = new TalkScheduleItem {Bell = true};
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

         TalkScheduleItem item = new TalkScheduleItem {Duration = testDur};
         Assert.AreEqual(item.Duration, testDur);
         Assert.AreEqual(item.OriginalDuration, testDur);
         Assert.IsTrue(item.GetDurationSeconds() == (int)testDur.TotalSeconds);

         item.Duration = changedDur;
         Assert.AreEqual(item.OriginalDuration, testDur);
         Assert.IsTrue(item.GetDurationSeconds() == (int)changedDur.TotalSeconds);
      }
   }
}
