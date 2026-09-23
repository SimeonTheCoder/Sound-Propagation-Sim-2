using System.Numerics;
using Utils;
using static Shared;

namespace Simulator;

public class Simulation
{
    public float[] waveformL = new float[SampleRate];
    public float[] waveformR = new float[SampleRate];

    public int RecordingDuration = 1;
    public float Scale = 1f;

    public bool SmoothingEnabled = false;
    public int KernelSize = 2;
    public float SmoothingPower = 2;
    public float ReflectionCoefficient = 0.7f;

    public Vector2 RoomDimensions = new(1f, 1f);

    public void Calculate()
    {
        waveformL = new float[SampleRate * RecordingDuration];
        waveformR = new float[SampleRate * RecordingDuration];

        for (int i = -ReflectionsCount; i < ReflectionsCount; i ++)
        {
            for (int j = -ReflectionsCount; j < ReflectionsCount; j ++)
            {
                int currReflectionCount = Math.Abs(i) + Math.Abs(j);

                Vector2 targetPos = MathUtils.GetReflectedCoords(ListenerPos, j, i, RoomDimensions);
                
                float distance = Vector2.Distance(targetPos, SourcePos) * Scale;

                Vector2 incomingDirection = Vector2.Normalize(targetPos - SourcePos);
                if (j % 2 != 0) incomingDirection = new(-incomingDirection.X, incomingDirection.Y);
                if (i % 2 != 0) incomingDirection = new(incomingDirection.X, -incomingDirection.Y);

                float l = MathF.Max(0f, -incomingDirection.Y);
                float r = MathF.Max(0f, incomingDirection.Y);

                float amplitude = 1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, currReflectionCount);

                int index = (int) (distance * Scale / SpeedOfSound * SampleRate);
                if (index >= SampleRate * RecordingDuration) continue;

                if (SmoothingEnabled)
                {
                    for (int k = 0; k < KernelSize; k ++)
                    {
                        float factor = MathF.Pow((KernelSize - MathF.Abs(k - 0)) / KernelSize, SmoothingPower);
                        if (index + k < 0 || index + k >= SampleRate * RecordingDuration) continue;
                        waveformL[index + k] += amplitude * factor * l;
                        waveformR[index + k] += amplitude * factor * r;
                    }
                }
                else
                {
                    waveformL[index] += amplitude * l;
                    waveformR[index] += amplitude * r;
                }
            }
        }
    }
}