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

        CalculateRecursive(graph, ListenerPos, 0f, 360f, 0, -1);

        // for (int k = 0; k < Count; k++)
        // {
        //     graph.GenerateData(Steps, k);

        //     for (int i = 0; i < graph.data.Count; i++)
        //     {
        //         bool valid = graph.IsValidPath(i, ListenerPos, k);

        //         Vector2 newMic = graph.TransformNodeWithGraph(ListenerPos, graph.data[i], k);

        //         if (valid)
        //         {
        //             float distance = Vector2.Distance(newMic, new(0f, 0f)) * Scale;
        //             Vector2 incomingDirection = Vector2.Normalize(newMic - new Vector2(0f, 0f));

        //             for (int m = 0; m < k; m++)
        //             {
        //                 (int from, int to) currEdge = graph.edges[graph.data[i].edges[m]];

        //                 Vector2 a = graph.nodes[currEdge.from];
        //                 Vector2 b = graph.nodes[currEdge.to];

        //                 incomingDirection = MathUtils.DoAxisFlip(
        //                     incomingDirection,
        //                     MathUtils.GetAngle(b - a),
        //                     0f,
        //                     0f
        //                 );
        //             }

        //             float l = MathF.Max(0f, -incomingDirection.Y);
        //             float r = MathF.Max(0f, incomingDirection.Y);

        //             float amplitude =
        //                 1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, k);

        //             int index = (int)(distance * Scale / SpeedOfSound * SampleRate);
        //             if (index >= SampleRate * RecordingDuration)
        //                 continue;

        //             WaveformL[index] += amplitude * l;
        //             WaveformR[index] += amplitude * r;
        //         }
        //     }
        // }
    }

    public void CalculateRecursive(
        Graph graph,
        Vector2 mic,
        float angleStart,
        float angleEnd,
        int depth,
        int lastEdge
    )
    {
        if (depth > Count)
            return;

        float angleToMic = MathUtils.GetAngle(mic);

        if (angleToMic >= angleStart && angleToMic <= angleEnd)
        {
            float distance = mic.Length() * Scale;
            float amplitude =
                1 / (0.05f + distance * distance) * MathF.Pow(ReflectionCoefficient, depth);

            int index = (int)(distance * Scale / SpeedOfSound * SampleRate);

            if (index < SampleRate * RecordingDuration)
            {
                WaveformL[index] += amplitude * 1;
                WaveformR[index] += amplitude * 1;
            }
        }

        for (int i = 0; i < graph.edges.Count; i++)
        {
            if (i == lastEdge)
                continue;

            int fromIndex = graph.edges[i].from;
            int toIndex = graph.edges[i].to;

            if (depth % 2 != 0)
            {
                int temp = fromIndex;
                fromIndex = toIndex;
                toIndex = temp;
            }

            Vector2 from = graph.nodes[fromIndex];
            Vector2 to = graph.nodes[toIndex];

            float start = MathUtils.GetAngle(from);
            float end = MathUtils.GetAngle(to);

            if (start < end && MathF.Max(start, angleStart) > MathF.Min(end, angleEnd))
                continue;
            else if (angleStart > end && angleEnd < start)
                continue;

            if (start < end && MathF.Min(angleEnd, end) > MathF.Max(angleStart, start))
            {
                CalculateRecursive(
                    Graph.MirrorGraphAlongAxis(graph, i),
                    Graph.MirrorNodeAlongAxis(graph, mic, i),
                    MathF.Max(angleStart, start),
                    MathF.Min(angleEnd, end),
                    depth + 1,
                    i
                );
            }
            else
            {
                if (start < angleEnd)
                {
                    CalculateRecursive(
                        Graph.MirrorGraphAlongAxis(graph, i),
                        Graph.MirrorNodeAlongAxis(graph, mic, i),
                        start,
                        angleEnd,
                        depth + 1,
                        i
                    );
                }

                if (angleStart < end)
                {
                    CalculateRecursive(
                        Graph.MirrorGraphAlongAxis(graph, i),
                        Graph.MirrorNodeAlongAxis(graph, mic, i),
                        angleStart,
                        end,
                        depth + 1,
                        i
                    );
                }
            }
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
