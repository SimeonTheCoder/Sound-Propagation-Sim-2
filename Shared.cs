using System.Numerics;
using Simulator;

public static class Shared
{
    public const int SampleRate = 48_000;
    public const float SpeedOfSound = 343;
    public static int ReflectionsCount = 100;

    public static Simulation SimRef = null!;

    public static Vector2 SourcePos = new(0.5f, 0.5f);
    public static Vector2 ListenerPos = new(0.5f, 0.5f);

    public static float Zoom = 100f;

    public static void InitShared()
    {
        Zoom = 100f;
        SimRef = new();
    }
}