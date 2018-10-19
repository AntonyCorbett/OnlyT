namespace OnlyT.Services.Bell
{
    public interface IBellService
    {
        bool IsPlaying { get; }

        void Play(int volumePercent);
    }
}
