using System.Threading.Tasks;

namespace OnlyT.Services.Bell;

/// <summary>
/// Manages the bell
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class BellService : IBellService
{
    private readonly TimerBell _bell;

    public BellService()
    {
        _bell = new TimerBell();
    }

    public bool IsPlaying => _bell.IsPlaying;

    public void Play(bool isEnabled, int volumePercent)
    {
        if (!isEnabled)
        {
            return;
        }

        Task.Run(() => _bell.Play(volumePercent));
    }
}