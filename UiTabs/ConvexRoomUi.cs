namespace UiTabs;

using System.Numerics;
using Graphs;
using Raylib_cs;
using Utils;

using static Shared;
using static Raylib_cs.Raylib;
using static Utils.RenderingUtils;
using static Utils.MathUtils;
using ImGuiNET;

public class ConvexRoomUi : IUi
{
    bool placementMode = false;
    bool doSnapping = false;

    private Graph graph = null!;
    private GraphPath selectedPath = null!;

    int Count = 16;
    int Steps = 720;

    public void Draw()
    {
        Vector2 mousePos = GetMouseWorldPos();
        Rect(mousePos, new(0.1f, 0.1f), Color.Yellow);

        ClearBackground(Color.Black);

        if (!placementMode && graph.nodes.Count > 0)
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

            Console.WriteLine($"Valid: {(validCount / (float) graph.data.Count * 100):f2}%");
        }

        DrawGraph(graph, selectedPath.Length > 0 ? selectedPath.edges[selectedPath.Length - 1] : -1);

        if (selectedPath.Length > 0)
        {
            for (int i = 0; i < selectedPath.Length; i++)
            {
                DrawGraph(graph, selectedPath.edges[i]);

                Rect(
                    RotateVec(
                        graph.TransformNodeWithGraph(ListenerPos, selectedPath, i + 1),
                        Theta,
                        DoRotation
                    ),
                    new(0.2f, 0.2f),
                    Color.Red
                );
            }
        }

        if (placementMode && graph.nodes.Count > 0)
            Line(graph.nodes[graph.nodes.Count - 1], mousePos, Color.Yellow);

        foreach (Vector2 node in graph.nodes)
        {
            if (Vector2.Distance(node, mousePos) < 0.1f)
                Rect(node, new(0.2f, 0.2f), Color.Green);
        }

        Rect(ListenerPos, new(0.2f, 0.2f), Color.Red);

        Line(
            RotateVec(new(0, -100), -Theta, !DoRotation),
            RotateVec(new(0, 100), -Theta, !DoRotation),
            Color.Green
        );
    }

    public bool DrawUi()
    {
        if (!ImGui.BeginTabItem("Convex Room")) return false;

        ImGui.Checkbox("Snap Mouse", ref doSnapping);
        ImGui.SliderFloat("Theta", ref Theta, 0f, 360f);
        ImGui.SliderInt("Step Size", ref Steps, 0, 360_0);
        ImGui.SliderInt("Count", ref Count, 0, 100);
        ImGui.Checkbox("Point up", ref DoRotation);

        ImGui.BeginMultiSelect(ImGuiMultiSelectFlags.SingleSelect);

        for (int i = 0; i < graph.edges.Count; i++)
        {
            if (ImGui.Selectable($"{i}: {graph.edges[i].from} --> {graph.edges[i].to}"))
                selectedPath.edges.Add(i);
        }

        ImGui.EndMultiSelect();

        ImGui.EndTabItem();
        return true;
    }

    public void Init()
    {
        Pan = new(0f, 0f);

        graph = new();
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
        if (!placementMode && graph.nodes.Count > 0)
            selectedPath = Graph.GeneratePath(graph, Count, Theta);

        Vector2 mousePos = GetMouseWorldPos();
        Rect(mousePos, new(0.1f, 0.1f), Color.Yellow);

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