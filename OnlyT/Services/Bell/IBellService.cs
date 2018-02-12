namespace OnlyT.Services.Bell
{
    public interface IBellService
    {
        void Play(int volumePercent);
        
        bool IsPlaying { get; }
    }
}
