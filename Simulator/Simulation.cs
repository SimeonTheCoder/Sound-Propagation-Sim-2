using System.Numerics;
using ImGuiNET;
using Utils;
using static Shared;

namespace Simulator;

public class Simulation : ISim
{
    public float[] WaveformL {get; set;} = new float[SampleRate];
    public float[] WaveformR {get; set;} = new float[SampleRate];

    public object Room {get; set;} = new Vector2(1f, 1f);

    public void Calculate()
    {
        WaveformL = new float[SampleRate * RecordingDuration];
        WaveformR = new float[SampleRate * RecordingDuration];

        for (int i = -ReflectionsCount; i < ReflectionsCount; i ++)
        {
            for (int j = -ReflectionsCount; j < ReflectionsCount; j ++)
            {
                int currReflectionCount = Math.Abs(i) + Math.Abs(j);

                Vector2 targetPos = MathUtils.GetReflectedCoords(ListenerPos, j, i, (Vector2) Room);
                
                float distance = Vector2.Distance(targetPos, SourcePos) * Scale;

                Vector2 incomingDirection = Vector2.Normalize(targetPos - SourcePos);
                if (j % 2 != 0) incomingDirection = new(-incomingDirection.X, incomingDirection.Y);
                if (i % 2 != 0) incomingDirection = new(incomingDirection.X, -incomingDirection.Y);

                float l = MathF.Max(0f, -incomingDirection.Y);
                float r = MathF.Max(0f, incomingDirection.Y);

                float amplitude = 1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, currReflectionCount);

                int index = (int) (distance * Scale / SpeedOfSound * SampleRate);
                if (index >= SampleRate * RecordingDuration) continue;

                WaveformL[index] += amplitude * l;
                WaveformR[index] += amplitude * r;
            }
        }

        if (Smoothing.SmoothingEnabled)
        {
            WaveformL = Smoothing.Smooth(WaveformL);
            WaveformR = Smoothing.Smooth(WaveformR);
        }
    }

    private Vector2 RoomDimensions = new(1f, 1f);

    public void DrawControls()
    {
        ImGui.InputFloat2("Room Dimensions", ref RoomDimensions);
        this.Room = RoomDimensions;
    }
}