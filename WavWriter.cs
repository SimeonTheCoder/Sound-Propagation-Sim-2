using System.Text;

using static Shared;

public class WavWriter
{
    public static void Write(float[] samplesL, float[] samplesR)
    {
        float max = MathF.Max(samplesL.Max(), samplesR.Max()); 

        List<byte> bytes =
        [
            .. ConvertString("RIFF"),
            .. ConvertInt(20 + (uint) samplesL.Length * 4 * 2), //filesize - 8 bytes
            .. ConvertString("WAVE"),
            .. ConvertString("fmt "), //format chunk
            .. ConvertInt(16), //chunk size = 4 + 4 + 2 + 2 + 4 + 4 + 2 + 2 - 8 = 24 - 8 = 16
            .. ConvertShort(3), // float
            .. ConvertShort(2), // 2 channels
            .. ConvertInt(SampleRate), //48 kHz
            .. ConvertInt(SampleRate * 32 * 2 / 8), //bytes per second = (Sample Rate * BitsPerSample * Channels) / 8
            .. ConvertShort(2 * 32 / 8), //bytes per block
            .. ConvertShort(32), //bits per sample
            .. ConvertString("data"), //data chunk
            .. ConvertInt((uint) samplesL.Length * 4 * 2) //data size
        ];

        for (int i = 0; i < samplesL.Length; i ++)
        {
            bytes.AddRange(BitConverter.GetBytes(samplesL[i] / max));
            bytes.AddRange(BitConverter.GetBytes(samplesR[i] / max));
        }

        File.WriteAllBytes("output.wav", bytes.ToArray());
    }

    private static byte[] ConvertString(string str)
    {
        return str.ToCharArray().Select(c => (byte) c).Take(str.Length).ToArray();
    }

    private static byte[] ConvertShort(ushort num)
    {
        return [
            (byte) (num & 0xFF),
            (byte) (num >> 8),
        ];
    }

    private static byte[] ConvertInt(uint num)
    {
        return [
            (byte) (num & 0xFF),
            (byte) ((num >> 8) & 0xFF),
            (byte) ((num >> 16) & 0xFF),
            (byte) (num >> 24),
        ];
    }
}