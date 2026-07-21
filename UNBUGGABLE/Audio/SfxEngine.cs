namespace UNBEATABLEChartEditor.Audio;

using System.Collections.Generic;

/// <summary>
/// NAudio helper used for playing hit sounds. I'm using two audio packages (NAudio and LibVLC) in
/// UNBUGGABLE because LibVLC doesn't let me have multiple things playing at once (so I can't use it
/// for hit sounds), but NAudio really likes to get desynced.
/// </summary>
public static class SfxEngine
{
    public static float Volume
    {
        // all engines will always have the same volume
        get => _playbackEngines[0].Volume;
        set
        {
            foreach (var engine in _playbackEngines)
            {
                engine.Volume = value;
            }
        }
    }

    private static List<CachedAudioPlaybackEngine> _playbackEngines = [];
    private static int _currentPlaybackEngineIndex = 0;

    public static void Init(int maxConcurrentSfx = 10)
    {
        for (int i = 0; i < maxConcurrentSfx; i++)
        {
            _playbackEngines.Add(new CachedAudioPlaybackEngine());
        }
    }

    public static void Play(CachedSound sound, long offset)
    {
        _playbackEngines[_currentPlaybackEngineIndex++ % _playbackEngines.Count]
            .Play(sound, offset);
    }

    public static void DisposeInstances()
    {
        foreach (var engine in _playbackEngines)
        {
            engine.Dispose();
        }
    }
}