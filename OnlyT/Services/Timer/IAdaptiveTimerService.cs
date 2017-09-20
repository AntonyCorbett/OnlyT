namespace OnlyT.Services.Timer
{
   using System;

   public interface IAdaptiveTimerService
   {
      TimeSpan? CalculateAdaptiveDuration(int itemId);
   }
}
