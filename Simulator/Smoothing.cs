namespace Simulator;

public class Smoothing
{
    public static bool SmoothingEnabled = false;
    public static int KernelSize = 2;
    public static float SmoothingPower = 2;
    public static bool Bidirectional = false;

    public static float[] Smooth(float[] waveform)
    {
        float[] result = new float[waveform.Length];

        int kernelStart = Bidirectional ? -KernelSize : 0;

        for (int index = 0; index < waveform.Length; index ++)
        {
            for (int k = kernelStart; k < KernelSize; k ++)
            {
                float factor = MathF.Pow((KernelSize - MathF.Abs(k - 0)) / KernelSize, SmoothingPower);
                if (index + k < 0 || index + k >= waveform.Length) continue;

                result[index + k] += waveform[index] * factor;
            }
        }

        return result;
    }
}