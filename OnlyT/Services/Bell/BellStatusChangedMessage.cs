namespace OnlyT.Services.Bell
{
   internal class BellStatusChangedMessage
   {
      public bool Playing { get; }

      public BellStatusChangedMessage(bool playing)
      {
         Playing = playing;
      }
   }
}
