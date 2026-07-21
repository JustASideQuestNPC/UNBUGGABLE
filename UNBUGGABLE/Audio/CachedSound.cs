using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace UNBEATABLEChartEditor.Audio;

public class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }

    public CachedSound(string filePath)
    {
        using (var fileReader = new AudioFileReader(filePath))
        {
            WaveFormat = fileReader.WaveFormat;
            var fullAudio = new List<float>((int)(fileReader.Length / 4));
            var buffer = new float[WaveFormat.Channels * WaveFormat.SampleRate];
            int samplesRead;
            while ((samplesRead = fileReader.Read(buffer, 0, buffer.Length)) > 0)
            {
                fullAudio.AddRange(buffer.Take(samplesRead));
            }
            AudioData = fullAudio.ToArray();
        }
    }
}