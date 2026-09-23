using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

public class Program
{
    const int SampleRate = 48_000;
    const float SpeedOfSound = 343;

    static int Width = 1920;
    static int Height = 1080;

    static Vector2 RoomDimensions = new(1f, 1f);

    static float Zoom = 100f;
    static float TimeScale = 0f;
    static float Scale = 1f;
    static float ReflectionCoefficient = 0.7f;
    static int RecordingDuration = 1;
    static int ReflectionsCount = 100;

    static (int x, int y) TransformCoords(Vector2 coords)
    {
        return (
            (int) ((coords.X - 0.5f) * Zoom + Width / 2),
            Height - (int) ((coords.Y - 0.5f) * Zoom + Height / 2)
        );
    }

    static void Line(Vector2 from, Vector2 to, Color color)
    {
        (int x1, int y1) = TransformCoords(from);
        (int x2, int y2) = TransformCoords(to);

        Raylib.DrawLine(x1, y1, x2, y2, color);
    }

    static void Rect(Vector2 center, Vector2 size, Color color)
    {
        (int x, int y) = TransformCoords(center);
        Raylib.DrawRectangle(x - (int) size.X / 2, y - (int) size.Y / 2, (int) size.X, (int) size.Y, color);
    }

    static Vector2 GetReflectedCoords(Vector2 originalPos, int x, int y)
    {
        return new(
            x % 2 == 0 ? x * RoomDimensions.X + originalPos.X : (x + 1) * RoomDimensions.X - originalPos.X,
            y % 2 == 0 ? y * RoomDimensions.Y + originalPos.Y : (y + 1) * RoomDimensions.Y - originalPos.Y
        );
    }

    static void Main(string[] args)
    {
        bool spawnOtherWorlds = false;
        bool drawTrails = false;
        bool drawGuide = true;
        bool drawLabels = true;

        bool singleParticleView = false;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

        Raylib.InitWindow(1920, 1080, "Test");
        Raylib.SetTargetFPS(120);

        rlImGui.Setup(true);

        Vector2 initialPos = new(0.5f, 0.5f);
        Vector2 initialVelocity = Vector2.Normalize(new(1.0f, 0.3f));

        List<Vector2> positions = [new(initialPos.X, initialPos.Y)];
        List<Vector2> velocities = [new(initialVelocity.X, initialVelocity.Y)];
        List<Vector2> boundariesX = [new(0, RoomDimensions.X)];
        List<Vector2> boundariesY = [new(0, RoomDimensions.Y)];

        Vector2 micPos = new(0.5f, 0.5f);

        int aimX = 0;
        int aimY = 0;

        float[] waveformL = new float[SampleRate];
        float[] waveformR = new float[SampleRate];

        bool useDb = true;
        int dbMin = -60;
        int dbMax = 0;

        bool doSmoothing = false;
        int smoothingKernelSize = 2;
        float smoothingPower = 2;

        while(!Raylib.WindowShouldClose())
        {
            Width = Raylib.GetScreenWidth();
            Height = Raylib.GetScreenHeight();

            Zoom *= Raylib.GetMouseWheelMoveV().Y * 0.1f + 1;

            float dt = Raylib.GetFrameTime() * TimeScale;

            int posCount = positions.Count;

            for (int i = 0; i < posCount; i ++)
            {
                Vector2 prevPos = new(positions[i].X, positions[i].Y);
                positions[i] += velocities[i] * dt;

                if (positions[i].X < boundariesX[i].X || positions[i].X > boundariesX[i].Y)
                {
                    if (spawnOtherWorlds)
                    {
                        positions.Add(new(positions[i].X, positions[i].Y));
                        velocities.Add(new(velocities[i].X, velocities[i].Y));

                        float dir = ((positions[i].X < boundariesX[i].X) ? -1 : 1) * RoomDimensions.X;

                        boundariesX.Add(new(boundariesX[i].X + dir, boundariesX[i].Y + dir));
                        boundariesY.Add(new(boundariesY[i].X, boundariesY[i].Y));
                    }

                    velocities[i] = new(-velocities[i].X, velocities[i].Y);
                    positions[i] = prevPos;
                }

                if (positions[i].Y < boundariesY[i].X || positions[i].Y > boundariesY[i].Y)
                {
                    if (spawnOtherWorlds)
                    {
                        positions.Add(new(positions[i].X, positions[i].Y));
                        velocities.Add(new(velocities[i].X, velocities[i].Y));

                        float dir = ((positions[i].Y < boundariesY[i].X) ? -1 : 1) * RoomDimensions.Y;

                        boundariesX.Add(new(boundariesX[i].X, boundariesX[i].Y));
                        boundariesY.Add(new(boundariesY[i].X + dir, boundariesY[i].Y + dir));
                    }

                    velocities[i] = new(velocities[i].X, -velocities[i].Y);
                    positions[i] = prevPos;
                }
            }

            Raylib.BeginDrawing();
            if(!drawTrails) Raylib.ClearBackground(Color.Black);

            if (singleParticleView)
            {
                for (int i = -10; i < 10; i ++)
                {
                    for (int j = -10; j < 10; j ++)
                    {
                        if(drawGuide)
                        {
                            Rect(
                                GetReflectedCoords(micPos, j, i),
                                new(4, 4),
                                Color.Red
                            );
                        }

                        if (!boundariesX.Any(b => b.X == j)) continue;
                        if (!boundariesY.Any(b => b.X == i)) continue;

                        (int x, int y) textPos = TransformCoords(new(j + 0.01f, i - 0.01f));
                        if(drawLabels) Raylib.DrawText($"({j},{i - 1})", textPos.x, textPos.y, (int) (0.03f * Zoom), Color.Gray);

                        Color c = (i == 0 && j == 0) ? Color.Yellow : Color.Gray;

                        Line(new(j, i), new(j + RoomDimensions.X, i), c);
                        Line(new(j, i), new(j, i + RoomDimensions.Y), c);
                        Line(new(j, i + RoomDimensions.Y), new(j + RoomDimensions.X, i + RoomDimensions.Y), c);
                        Line(new(j + RoomDimensions.X, i), new(j + RoomDimensions.X, i + RoomDimensions.Y), c);
                    }
                }

                for (int i = 0; i < positions.Count; i ++)
                {
                    Rect(positions[i], new(4, 4), Color.Blue);
                    Line(positions[i], positions[i] + velocities[i] * 0.1f, Color.Blue);
                }

                if (drawGuide) Line(initialPos - initialVelocity * 10, initialPos + initialVelocity * 10, Color.Pink);
            }
            else
            {
                if (useDb)
                {
                    for (int i = -100; i < 0; i += 10)
                    {
                        int h = (int) ((i - dbMin + 0f) / (dbMax - dbMin) * Height);

                        Raylib.DrawLine(0, Height - h, Width, Height - h, Color.Gray);
                        Raylib.DrawText($"{i} dB", Width - 100, Height - h, 20, Color.Yellow);
                    }
                }

                Raylib.BeginBlendMode(BlendMode.Additive);

                for (int i = 0; i < SampleRate * RecordingDuration; i ++)
                {
                    int hl = (int) (waveformL[i] * Height);
                    int hr = (int) (waveformR[i] * Height);

                    if (useDb)
                    {
                        float dbL = 20 * MathF.Log10(waveformL[i]);
                        hl = (int) ((dbL - dbMin) / (dbMax - dbMin) * Height);

                        float dbR = 20 * MathF.Log10(waveformR[i]);
                        hr = (int) ((dbR - dbMin) / (dbMax - dbMin) * Height);
                    }
                    
                    Raylib.DrawRectangle((int) ((i + 0f) / SampleRate / RecordingDuration * Width * Zoom), Height - hl, (int) Zoom, hl, new Color(1f, 0f, 0f));
                    Raylib.DrawRectangle((int) ((i + 0f) / SampleRate / RecordingDuration * Width * Zoom), Height - hr, (int) Zoom, hr, new Color(0f, 1f, 0f));
                }

                Raylib.EndBlendMode();
            }

            rlImGui.Begin();

            ImGui.Begin("Controls");

            ImGui.InputFloat2("Room Dimensions", ref RoomDimensions);

            ImGui.InputFloat2("Source Position", ref initialPos);
            ImGui.InputFloat2("Microphone Position", ref micPos);

            ImGui.Separator();

            ImGui.BeginTabBar("tab_bar");

            if (ImGui.BeginTabItem("Particle Reflection Visualization"))
            {
                if (!singleParticleView)
                {
                    singleParticleView = true;
                    Zoom = 100f;
                }

                ImGui.Checkbox("Spawn other worlds", ref spawnOtherWorlds);
                ImGui.SliderFloat("Zoom", ref Zoom, 25, 2000);
                ImGui.Checkbox("Draw trails", ref drawTrails);
                ImGui.Checkbox("Draw guide", ref drawGuide);
                ImGui.Checkbox("Labels", ref drawLabels);

                ImGui.SliderFloat("Speed", ref TimeScale, 0f, 1f);

                if (ImGui.Button("Play")) TimeScale = 1f;
                ImGui.SameLine();
                if (ImGui.Button("Pause")) TimeScale = 0f;

                ImGui.SliderInt("Aim X", ref aimX, -10, 10);
                ImGui.SliderInt("Aim Y", ref aimY, -10, 10);

                if (ImGui.Button("Aim"))
                {
                    Vector2 targetPos = GetReflectedCoords(micPos, aimX, aimY);
                    
                    velocities[0] = Vector2.Normalize(targetPos - positions[0]);
                    initialVelocity = new(velocities[0].X, velocities[0].Y);
                }

                ImGui.SameLine();

                if (ImGui.Button("Reset"))
                {
                    // initialPos = new(0.5f, 0.5f);
                    // initialVelocity = Vector2.Normalize(new(1.0f, 0.3f));

                    positions = [new(initialPos.X, initialPos.Y)];
                    velocities = [new(initialVelocity.X, initialVelocity.Y)];
                    boundariesX = [new(0, RoomDimensions.X)];
                    boundariesY = [new(0, RoomDimensions.Y)];

                    // micPos = new(0.5f, 0.5f);
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("IR Simulation"))
            {
                if (singleParticleView != false)
                {
                    singleParticleView = false;
                    Zoom = 1f;
                }
            
                ImGui.SliderFloat("Scale", ref Scale, 0f, 100f, "%f", ImGuiSliderFlags.Logarithmic);
                ImGui.SliderFloat("Reflection Coefficient", ref ReflectionCoefficient, 0f, 1f, "%f");

                if (ImGui.InputInt("Recording Duration", ref RecordingDuration))
                {
                    waveformL = new float[RecordingDuration * SampleRate];
                    waveformR = new float[RecordingDuration * SampleRate];
                }

                ImGui.InputInt("Reflections Count", ref ReflectionsCount);

                ImGui.Checkbox("Use Smoothing", ref doSmoothing);

                if (doSmoothing)
                {
                    ImGui.InputInt("Kernel size", ref smoothingKernelSize);
                    ImGui.SliderFloat("Smoothing Power", ref smoothingPower, 0f, 3f);
                }

                if (ImGui.Button("Calculate"))
                {
                    waveformL = new float[SampleRate * RecordingDuration];
                    waveformR = new float[SampleRate * RecordingDuration];

                    for (int i = -ReflectionsCount; i < ReflectionsCount; i ++)
                    {
                        for (int j = -ReflectionsCount; j < ReflectionsCount; j ++)
                        {
                            int currReflectionCount = Math.Abs(i) + Math.Abs(j);

                            Vector2 targetPos = GetReflectedCoords(micPos, j, i);
                            
                            float distance = Vector2.Distance(targetPos, initialPos) * Scale;

                            Vector2 incomingDirection = Vector2.Normalize(targetPos - initialPos);
                            if (j % 2 != 0) incomingDirection = new(-incomingDirection.X, incomingDirection.Y);
                            if (i % 2 != 0) incomingDirection = new(incomingDirection.X, -incomingDirection.Y);

                            float l = MathF.Max(0f, -incomingDirection.Y);
                            float r = MathF.Max(0f, incomingDirection.Y);

                            float amplitude = 1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, currReflectionCount);

                            int index = (int) (distance * Scale / SpeedOfSound * SampleRate);
                            if (index >= SampleRate * RecordingDuration) continue;

                            if (doSmoothing)
                            {
                                for (int k = 0; k < smoothingKernelSize; k ++)
                                {
                                    float factor = MathF.Pow((smoothingKernelSize - MathF.Abs(k - 0)) / smoothingKernelSize, smoothingPower);
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

                ImGui.SameLine();

                if (ImGui.Button("Export"))
                {
                    string[] lines = new string[SampleRate * RecordingDuration];

                    for (int i = 0; i < waveformL.Length; i ++)
                        lines[i] = $"{waveformL[i]},{waveformR[i]}";

                    File.WriteAllLines("output.csv", lines);
                }

                // ImGui.PlotHistogram("Waveform", ref waveform[0], (int) (SampleRate * RecordingDuration * waveformZoom), 0, "", 0, 1, new(1000, 100));

                ImGui.Checkbox("Use dB for Visualization", ref useDb);

                if (useDb)
                {
                    ImGui.SliderInt("Floor", ref dbMin, -100, 0);
                    ImGui.SliderInt("Ceiling", ref dbMax, -100, 0);
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();

            ImGui.End();

            rlImGui.End();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }   
}