using System.Diagnostics;
using ImGuiNET;
using Raylib_cs;
using static Shared;

namespace UiTabs;

public class SimUi : IUi
{
    private bool useDb = true;
    private int dbMin = -60;
    private int dbMax = 0;

    private Music ir;
    private Music songDry;
    private Music songWet;

    private float volumeDry = 0.5f;
    private float volumeWet = 0.5f;

    private bool playingProcessed = false;

    public void Init()
    {
        Zoom = 1f;
    }

    public void Update()
    {
        Raylib.SetMusicVolume(ir, volumeDry);
        Raylib.SetMusicVolume(songDry, playingProcessed ? 0f : volumeDry);
        Raylib.SetMusicVolume(songWet, playingProcessed ? volumeWet : 0f);

        if (Raylib.IsMusicStreamPlaying(ir))
        {
            Raylib.UpdateMusicStream(ir);
        }

        if (Raylib.IsMusicStreamPlaying(songDry))
        {
            Raylib.UpdateMusicStream(songDry);
            Raylib.UpdateMusicStream(songWet);
        }
    }

    public void Draw()
    {
        Raylib.ClearBackground(Color.Black);

        int Width = Raylib.GetScreenWidth();
        int Height = Raylib.GetScreenHeight();

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

        for (int i = 0; i < SimRef.waveformL.Length; i ++)
        {
            int hl = (int) (SimRef.waveformL[i] * Height);
            int hr = (int) (SimRef.waveformR[i] * Height);

            if (useDb)
            {
                float dbL = 20 * MathF.Log10(SimRef.waveformL[i]);
                hl = (int) ((dbL - dbMin) / (dbMax - dbMin) * Height);

                float dbR = 20 * MathF.Log10(SimRef.waveformR[i]);
                hr = (int) ((dbR - dbMin) / (dbMax - dbMin) * Height);
            }
            
            Raylib.DrawRectangle((int) ((i + 0f) / SampleRate / SimRef.RecordingDuration * Width * Zoom), Height - hl, (int) Zoom, hl, new Color(1f, 0f, 0f));
            Raylib.DrawRectangle((int) ((i + 0f) / SampleRate / SimRef.RecordingDuration * Width * Zoom), Height - hr, (int) Zoom, hr, new Color(0f, 1f, 0f));
        }

        Raylib.EndBlendMode();
    }

    public bool DrawUi()
    {
        if (!ImGui.BeginTabItem("IR Simulation")) return false;
        
        ImGui.SliderFloat("Scale", ref SimRef.Scale, 0f, 100f, "%f", ImGuiSliderFlags.Logarithmic);
        ImGui.SliderFloat("Reflection Coefficient", ref SimRef.ReflectionCoefficient, 0f, 1f, "%f");

        ImGui.InputInt("Recording Duration", ref SimRef.RecordingDuration);

        ImGui.InputInt("Reflections Count", ref ReflectionsCount);

        ImGui.Checkbox("Use Smoothing", ref SimRef.SmoothingEnabled);

        if (SimRef.SmoothingEnabled)
        {
            ImGui.InputInt("Kernel size", ref SimRef.KernelSize);
            ImGui.SliderFloat("Smoothing Power", ref SimRef.SmoothingPower, 0f, 3f);
        }

        if (ImGui.Button("Calculate"))
        {
            SimRef.Calculate();
        }

        ImGui.SameLine();

        if (ImGui.Button("Export"))
        {
            Raylib.UnloadMusicStream(ir);
            Raylib.UnloadMusicStream(songDry);
            Raylib.UnloadMusicStream(songWet);

            WavWriter.Write(SimRef.waveformL, SimRef.waveformR);

            ir = Raylib.LoadMusicStream("output.wav");
            songDry = Raylib.LoadMusicStream("song.mp3");

            Process process = new()
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/C ffmpeg -y -i song.mp3 -i output.wav -filter_complex \"[1]loudnorm[a];[0][a]afir\" processed.wav"
                }
            };

            process.Start();
            process.WaitForExit();

            songWet = Raylib.LoadMusicStream("processed.wav");

            // string[] lines = new string[SampleRate * SimRef.RecordingDuration];

            // for (int i = 0; i < SimRef.waveformL.Length; i ++)
            //     lines[i] = $"{SimRef.waveformL[i]},{SimRef.waveformR[i]}";

            // File.WriteAllLines("output.csv", lines);
        }

        ImGui.Checkbox("Use dB for Visualization", ref useDb);

        if (useDb)
        {
            ImGui.SliderInt("Floor", ref dbMin, -100, 0);
            ImGui.SliderInt("Ceiling", ref dbMax, -100, 0);
        }

        ImGui.Separator();

        if (ImGui.Button(Raylib.IsMusicStreamPlaying(ir) ? "Stop" : "Play IR"))
        {
            if (!Raylib.IsMusicStreamPlaying(ir))
            {
                Raylib.PlayMusicStream(ir);
            }
            else
            {
                Raylib.StopMusicStream(ir);
            }
        }

        if (ImGui.Button(Raylib.IsMusicStreamPlaying(songDry) ? "Stop Playback" : "Play song"))
        {
            if (Raylib.IsMusicStreamPlaying(songDry))
            {
                Raylib.StopMusicStream(songDry);
                Raylib.StopMusicStream(songWet);
            }
            else
            {
                Raylib.StopMusicStream(ir);

                Raylib.PlayMusicStream(songDry);
                Raylib.PlayMusicStream(songWet);
            }
        }

        if (ImGui.Button($"{(playingProcessed ? "IR off" : "IR on")}"))
        {
            playingProcessed = !playingProcessed;
        }

        ImGui.SliderFloat("Volume Dry", ref volumeDry, 0f, 1f);
        ImGui.SliderFloat("Volume Wet", ref volumeWet, 0f, 1f);

        ImGui.EndTabItem();
        return true;
    }
}