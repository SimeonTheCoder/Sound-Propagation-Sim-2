namespace UiTabs;

using System.Numerics;
using Graphs;
using ImGuiNET;
using Raylib_cs;
using Utils;
using static Raylib_cs.Raylib;
using static Shared;
using static Utils.MathUtils;
using static Utils.RenderingUtils;

public class ConvexRoomUi : IUi
{
    bool placementMode = false;
    bool doSnapping = false;
    bool recursiveMode = true;

    private Graph graph = null!;
    private GraphPath selectedPath = null!;

    int Count = 2;
    int Steps = 720;

    public void CalculateRecursive(
        Graph graph,
        Vector2 mic,
        Vector2 origin,
        float angleStart,
        float angleEnd,
        int depth,
        bool crossSegment
    )
    {
        if (depth > Count)
            return;

        float angleToMic = GetAngle(mic);

        if (!crossSegment)
        {
            if (angleToMic >= angleStart && angleToMic <= angleEnd)
            {
                Line(new(0f, 0f), mic, Color.Red);
            }
        }
        else
        {
            if (angleToMic >= angleStart || angleToMic <= angleEnd)
                Line(new(0f, 0f), mic, Color.Red);
        }

        for (int i = 0; i < graph.edges.Count; i++)
        {
            Vector2 from = graph.nodes[graph.edges[i].from];
            Vector2 to = graph.nodes[graph.edges[i].to];

            float start = GetAngle(from - origin);
            float end = GetAngle(to - origin);

            if (!crossSegment)
            {
                if (start < end && ((start < angleStart && end < angleStart) || (start > angleStart && start > angleEnd))) continue;
                else if (angleStart > end && angleEnd < start) continue;
            }

            if (!crossSegment)
            {
                if (start < end && MathF.Min(angleEnd, end) > MathF.Max(angleStart, start))
                {
                    CalculateRecursive(
                        Graph.MirrorGraphAlongAxis(graph, i),
                        Graph.MirrorNodeAlongAxis(graph, mic, i),
                        Graph.MirrorNodeAlongAxis(graph, origin, i),
                        MathF.Max(angleStart, start),
                        MathF.Min(angleEnd, end),
                        depth + 1,
                        false
                    );
                }
                else
                {
                    if (start < angleEnd)
                    {
                        CalculateRecursive(
                            Graph.MirrorGraphAlongAxis(graph, i),
                            Graph.MirrorNodeAlongAxis(graph, mic, i),
                            Graph.MirrorNodeAlongAxis(graph, origin, i),
                            start,
                            angleEnd,
                            depth + 1,
                            false
                        );
                    }

                    if (angleStart < end)
                    {
                        CalculateRecursive(
                            Graph.MirrorGraphAlongAxis(graph, i),
                            Graph.MirrorNodeAlongAxis(graph, mic, i),
                            Graph.MirrorNodeAlongAxis(graph, origin, i),
                            angleStart,
                            end,
                            depth + 1,
                            false
                        );
                    }
                }
            }
            else
            {
                if(start < end)
                {
                    if (start < angleEnd)
                    {
                        CalculateRecursive(
                            Graph.MirrorGraphAlongAxis(graph, i),
                            Graph.MirrorNodeAlongAxis(graph, mic, i),
                            Graph.MirrorNodeAlongAxis(graph, origin, i),
                            start,
                            angleEnd,
                            depth + 1,
                            true
                        );
                    }

                    if (angleStart < end)
                    {
                        CalculateRecursive(
                            Graph.MirrorGraphAlongAxis(graph, i),
                            Graph.MirrorNodeAlongAxis(graph, mic, i),
                            Graph.MirrorNodeAlongAxis(graph, origin, i),
                            angleStart,
                            end,
                            depth + 1,
                            true
                        );
                    }
                }
                else
                {
                    CalculateRecursive(
                        Graph.MirrorGraphAlongAxis(graph, i),
                        Graph.MirrorNodeAlongAxis(graph, mic, i),
                        Graph.MirrorNodeAlongAxis(graph, origin, i),
                        MathF.Max(start, angleStart),
                        MathF.Min(end, angleEnd),
                        depth + 1,
                        true
                    );
                }
            }
        }
    }

    public void Draw()
    {
        if (!(SimRef is ConvexSimulation))
            return;
        Vector2 mousePos = GetMouseWorldPos();
        // Rect(mousePos, new(0.1f, 0.1f), Color.Yellow);

        ClearBackground(Color.Black);

        if (!placementMode && graph.nodes.Count > 0)
        {
            if (recursiveMode)
            {
                CalculateRecursive(graph, ListenerPos, new(0f, 0f), 0f, 360f, 0, false);
            }
            else
            {
                graph.GenerateData(Steps, Count);
                int validCount = 0;

                for (int i = 0; i < graph.data.Count; i++)
                {
                    for (int k = 0; k < Count; k++)
                    {
                        bool valid = graph.IsValidPath(i, ListenerPos, k);

                        Vector2 newMic = graph.TransformNodeWithGraph(
                            ListenerPos,
                            graph.data[i],
                            k
                        );

                        if (valid)
                        {
                            validCount++;
                            Line(newMic, new(0, 0), Color.Red);
                        }
                    }
                }
            }
        }

        DrawGraph(
            graph,
            selectedPath.Length > 0 ? selectedPath.edges[selectedPath.Length - 1] : -1
        );

        // for (int i = 0; i < graph.edges.Count; i++)
        // {
        //     Vector2 from = graph.nodes[graph.edges[i].from];
        //     Vector2 to = graph.nodes[graph.edges[i].to];

        //     float start = GetAngle(from);
        //     float end = GetAngle(to);

        //     bool collision =
        //         (start <= Theta && end >= Theta && start < end)
        //         || (end < start && (end >= Theta || Theta >= start));

        //     (int x, int y) coords = TransformCoords(from * 0.5f + to * 0.5f);
        //     DrawText(
        //         $"{start:f1} to {end:f1}",
        //         coords.x,
        //         coords.y + 20,
        //         20,
        //         collision ? Color.Red : Color.Yellow
        //     );

        //     Line(
        //         new(0f, 0f),
        //         RotateVec(from, Theta, DoRotation),
        //         collision ? Color.Red : Color.Yellow
        //     );
        //     Line(
        //         new(0f, 0f),
        //         RotateVec(to, Theta, DoRotation),
        //         collision ? Color.Red : Color.Yellow
        //     );
        // }

        if (selectedPath.Length > 0)
        {
            Graph curr = graph;

            for (int i = 0; i < selectedPath.Length; i++)
            {
                curr = Graph.MirrorGraphAlongAxis(curr, selectedPath.edges[i]);
                DrawGraph(curr, selectedPath.edges[i]);

                Rect(
                    RotateVec(
                        graph.TransformNodeWithGraph(ListenerPos, selectedPath, i + 1),
                        Theta,
                        DoRotation
                    ),
                    new(0.05f, 0.05f),
                    Color.Red
                );
            }
        }

        if (placementMode && graph.nodes.Count > 0)
            Line(graph.nodes[graph.nodes.Count - 1], mousePos, Color.Yellow);

        foreach (Vector2 node in graph.nodes)
        {
            if (Vector2.Distance(node, mousePos) < 0.1f)
                Rect(node, new(0.05f, 0.05f), Color.Green);
        }

        Rect(ListenerPos, new(0.05f, 0.05f), Color.Red);
        Rect(new(0f, 0f), new(0.05f, 0.05f), Color.Blue);

        Line(
            RotateVec(new(0, -100), -Theta, !DoRotation),
            RotateVec(new(0, 100), -Theta, !DoRotation),
            Color.Green
        );

        Line(
            RotateVec(new(-100, 0), Theta, !DoRotation),
            RotateVec(new(100, 0), Theta, !DoRotation),
            Color.Green
        );
    }

    public bool DrawUi()
    {
        if (!ImGui.BeginTabItem("Convex Room"))
            return false;

        if (!(SimRef is ConvexSimulation))
        {
            ImGui.TextUnformatted("Unsupported simulation format!");

            ImGui.EndTabItem();
            return false;
        }

        ImGui.Checkbox("Snap Mouse", ref doSnapping);
        ImGui.SliderFloat("Theta", ref Theta, 0f, 360f);
        ImGui.SliderInt("Step Size", ref Steps, 0, 360_0);
        ImGui.SliderInt("Count", ref Count, 0, 100);
        ImGui.Checkbox("Point up", ref DoRotation);
        ImGui.Checkbox("Recursive mode", ref recursiveMode);

        ImGui.BeginMultiSelect(ImGuiMultiSelectFlags.SingleSelect);

        for (int i = 0; i < graph.edges.Count; i++)
        {
            if (ImGui.Selectable($"{i}: {graph.edges[i].from} --> {graph.edges[i].to}"))
                selectedPath.edges.Add(i);
        }

        SimRef.Room = graph;

        ImGui.EndMultiSelect();

        ImGui.EndTabItem();
        return true;
    }

    public void Init()
    {
        Pan = new(0f, 0f);

        if (graph == null)
            graph = new();
        if (selectedPath == null)
            selectedPath = new();
    }

    private Vector2 GetMouseWorldPos()
    {
        Vector2 mousePos = GetMousePosition();
        mousePos = ClipToXY(((int)mousePos.X, (int)mousePos.Y));

        return doSnapping ? new(MathF.Round(mousePos.X), MathF.Round(mousePos.Y)) : mousePos;
    }

    public void Update()
    {
        if (!(SimRef is ConvexSimulation))
            return;

        if (!placementMode && graph.nodes.Count > 0)
            selectedPath = Graph.GeneratePath(graph, Count, Theta);

        Vector2 mousePos = GetMouseWorldPos();

        if (IsKeyPressed(KeyboardKey.P))
            placementMode = !placementMode;

        if (placementMode && IsMouseButtonPressed(MouseButton.Left))
        {
            int snapNode = -1;

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (Vector2.Distance(graph.nodes[i], mousePos) >= 0.1f)
                    continue;

                snapNode = i;
                break;
            }

            if (graph.nodes.Count > 0)
                graph.edges.Add(
                    (graph.nodes.Count - 1, (snapNode != -1) ? snapNode : graph.nodes.Count)
                );
            if (snapNode == -1)
                graph.nodes.Add(mousePos);
        }
    }
}
