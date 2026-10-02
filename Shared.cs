using System.Numerics;
using Simulator;

public static class Shared
{
    public const int SampleRate = 48_000;
    public const float SpeedOfSound = 343;
    public static int ReflectionsCount = 100;

    public static ISim SimRef = null!;

    public static int RecordingDuration = 1;
    public static float Scale = 1f;

    public static float ReflectionCoefficient = 0.7f;

    public static Vector2 SourcePos = new(0.5f, 0.5f);
    public static Vector2 ListenerPos = new(0.5f, 0.5f);

    public static float Zoom = 100f;
    public static Vector2 Pan = new(0f, 0f);

    public static void InitShared()
    {
        Zoom = 100f;
    }
}