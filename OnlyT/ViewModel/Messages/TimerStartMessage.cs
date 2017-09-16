namespace OnlyT.ViewModel.Messages
{
   /// <summary>
   /// When the timer is started
   /// </summary>
   internal class TimerStartMessage
   {
      public int TargetSeconds { get; }

      public TimerStartMessage(int targetSeconds)
      {
         TargetSeconds = targetSeconds;
      }
   }
}
