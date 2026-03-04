using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int sampleCount = clip.samples * clip.channels;
            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);

            byte[] wavData = ConvertAndWrite(samples, clip.channels, clip.frequency);
            return wavData;
        }
    }

    static byte[] ConvertAndWrite(float[] samples, int channels, int hz)
    {
        MemoryStream stream = new MemoryStream();

        int sampleCount = samples.Length;
        int byteCount = sampleCount * 2;

        // RIFF header
        WriteString(stream, "RIFF");
        WriteInt(stream, HEADER_SIZE + byteCount - 8);
        WriteString(stream, "WAVE");

        // fmt chunk
        WriteString(stream, "fmt ");
        WriteInt(stream, 16);
        WriteShort(stream, 1);
        WriteShort(stream, (short)channels);
        WriteInt(stream, hz);
        WriteInt(stream, hz * channels * 2);
        WriteShort(stream, (short)(channels * 2));
        WriteShort(stream, 16);

        // data chunk
        WriteString(stream, "data");
        WriteInt(stream, byteCount);

        foreach (var sample in samples)
        {
            short intSample = (short)(sample * short.MaxValue);
            WriteShort(stream, intSample);
        }

        return stream.ToArray();
    }

    static void WriteString(Stream stream, string value)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    static void WriteInt(Stream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 4);
    }

    static void WriteShort(Stream stream, short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 2);
    }
}