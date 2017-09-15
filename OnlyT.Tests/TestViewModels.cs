using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.ViewModel;

namespace OnlyT.Tests
{
   [TestClass]
   public class TestViewModels
   {
      [TestMethod]
      public void TestMainViewModel()
      {
         Mock<IOptionsService> optionsService = new Mock<IOptionsService>();
         Mock<IMonitorsService> monitorsService = new Mock<IMonitorsService>();
         Mock<IBellService> bellService = new Mock<IBellService>();

         //MainViewModel viewModel = new MainViewModel(optionsService.Object, monitorsService.Object, bellService.Object);
      }
   }
}
