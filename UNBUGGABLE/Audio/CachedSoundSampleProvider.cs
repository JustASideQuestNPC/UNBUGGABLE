using System;
using NAudio.Wave;

namespace UNBEATABLEChartEditor.Audio;

public class CachedSoundSampleProvider(CachedSound cachedSound, float volume) : ISampleProvider
{
    private long _position;

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = cachedSound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        var destOffset = offset;
        for (var sourceSample = 0; sourceSample < samplesToCopy; ++sourceSample)
        {
            buffer[destOffset] = cachedSound.AudioData[_position + sourceSample] * volume;
            ++destOffset;
        }

        _position += samplesToCopy;
        return (int)samplesToCopy;
    }
    
    public WaveFormat WaveFormat => cachedSound.WaveFormat;
}