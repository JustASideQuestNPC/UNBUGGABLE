using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace UNBEATABLEChartEditor.Audio;

public class CachedAudioPlaybackEngine : IDisposable
{
    public float Volume { get; set; } = 1;

    private readonly IWavePlayer _waveOutput;
    private readonly MixingSampleProvider _mixer;

    public CachedAudioPlaybackEngine(int sampleRate = 44100, int channels = 2)
    {
        _waveOutput = new WasapiOut();
        _mixer = new MixingSampleProvider(
            WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels))
        {
            ReadFully = true
        };
        _waveOutput.Init(_mixer);
        _waveOutput.Play();
    }

    public void Play(CachedSound sound, long offset)
    {
        var sampleProvider = new CachedSoundSampleProvider(sound, Volume);
        sampleProvider.Skip(TimeSpan.FromMilliseconds(offset));
        AddMixerInput(sampleProvider);
    }
    
    public void Dispose()
    {
        _waveOutput.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
        {
            return input;
        }
        if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
        {
            return new MonoToStereoSampleProvider(input);
        }
        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }
    
    private void AddMixerInput(ISampleProvider sampleProvider)
    {
        _mixer.AddMixerInput(ConvertToRightChannelCount(sampleProvider));
    }
}