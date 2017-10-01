using System.Threading.Tasks;

namespace OnlyT.Services.Bell
{
    /// <summary>
    /// Manages the bell
    /// </summary>
    public class BellService : IBellService
    {
        private readonly TimerBell _bell;

        public BellService()
        {
            _bell = new TimerBell();
        }

        public bool IsPlaying => _bell.IsPlaying;

        public void Play(int volumePercent)
        {
            Task.Run(() =>
            {
                _bell.Play(volumePercent);
            });
        }

    }
}
