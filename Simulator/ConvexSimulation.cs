using System.Numerics;
using Graphs;
using ImGuiNET;
using Simulator;
using Utils;
using static Shared;

public class ConvexSimulation : ISim
{
    public float[] WaveformL { get; set; } = new float[SampleRate];
    public float[] WaveformR { get; set; } = new float[SampleRate];
    public object Room { get; set; } = null!;

    public void Calculate()
    {
        WaveformL = new float[SampleRate * RecordingDuration];
        WaveformR = new float[SampleRate * RecordingDuration];

        Graph graph = (Graph)Room;

        for (int k = 0; k < Count; k++)
        {
            graph.GenerateData(Steps, k);

            for (int i = 0; i < graph.data.Count; i++)
            {
                bool valid = graph.IsValidPath(i, ListenerPos, k);

                Vector2 newMic = graph.TransformNodeWithGraph(ListenerPos, graph.data[i], k);

                if (valid)
                {
                    float distance = Vector2.Distance(newMic, new(0f, 0f)) * Scale;
                    Vector2 incomingDirection = Vector2.Normalize(newMic - new Vector2(0f, 0f));

                    for (int m = 0; m < k; m++)
                    {
                        (int from, int to) currEdge = graph.edges[graph.data[i].edges[m]];

                        Vector2 a = graph.nodes[currEdge.from];
                        Vector2 b = graph.nodes[currEdge.to];

                        incomingDirection = MathUtils.DoAxisFlip(
                            incomingDirection,
                            MathUtils.GetAngle(b - a),
                            0f,
                            0f
                        );
                    }

                    float l = MathF.Max(0f, -incomingDirection.Y);
                    float r = MathF.Max(0f, incomingDirection.Y);

                    float amplitude =
                        1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, k);

                    int index = (int)(distance * Scale / SpeedOfSound * SampleRate);
                    if (index >= SampleRate * RecordingDuration)
                        continue;

                    WaveformL[index] += amplitude * l;
                    WaveformR[index] += amplitude * r;
                }
            }
        }
    }

    public void CalculateRecursive()
    {
        for (int k = 0; k < Count; k ++)
        {
            
        }
    }

    private int Steps = 720;
    private int Count = 16;

    public void DrawControls()
    {
        ImGui.SliderInt("Step Size", ref Steps, 0, 360_0);
        ImGui.SliderInt("Count", ref Count, 0, 100);
    }
}
