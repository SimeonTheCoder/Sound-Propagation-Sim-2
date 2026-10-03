using System.Numerics;
using Graphs;
using Raylib_cs;

using static Shared;

namespace Utils;

public class RenderingUtils
{
    public static float Theta = 0;
    public static bool DoRotation = false;

    public static (int x, int y) TransformCoords(Vector2 coords)
    {
        return (
            (int) ((coords.X - 0.5f + Pan.X) * Zoom + Raylib.GetScreenWidth() / 2),
            Raylib.GetScreenHeight() - (int) ((coords.Y - 0.5f + Pan.Y) * Zoom + Raylib.GetScreenHeight() / 2)
        );
    }

    public static Vector2 ClipToXY((int x, int y) clip, bool ignorePan = false)
    {
        return new(
            (clip.x - Raylib.GetScreenWidth() / 2) / Zoom + 0.5f - (ignorePan ? 0 : Pan.X),
            (Raylib.GetScreenHeight() / 2 - clip.y) / Zoom + 0.5f - (ignorePan ? 0 : Pan.Y)
        );
    }

    public static void Line(Vector2 from, Vector2 to, Color color)
    {
        (int x1, int y1) = TransformCoords(from);
        (int x2, int y2) = TransformCoords(to);

        Raylib.DrawLine(x1, y1, x2, y2, color);
    }

    public static void Rect(Vector2 center, Vector2 size, Color color)
    {
        (int x, int y) = TransformCoords(center);
        
        Raylib.DrawRectangle(
            x - (int)(size.X / 2 * Zoom),
            y - (int)(size.Y / 2 * Zoom),
            (int)(size.X * Zoom),
            (int)(size.Y * Zoom),
            color
        );
    }

    public static void DrawGraph(Graph graph, int selectedEdge)
    {
        DrawLinks(graph, selectedEdge);
        DrawNodes(graph);
    }

    public static void DrawNodes(Graph graph)
    {
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            Rect(
                MathUtils.RotateVec(graph.nodes[i], Theta, DoRotation),
                new(0.1f, 0.1f),
                Color.White
            );

            (int x, int y) coords = TransformCoords(
                MathUtils.RotateVec(graph.nodes[i], Theta, DoRotation)
            );
            Raylib.DrawText($"{i}", coords.x - 10, coords.y - 20, 10, Color.Yellow);
        }
    }

    public static void DrawLinks(Graph graph, int selectedEdge)
    {
        for (int i = 0; i < graph.edges.Count; i++)
        {
            Line(
                MathUtils.RotateVec(graph.nodes[graph.edges[i].from], Theta, DoRotation),
                MathUtils.RotateVec(graph.nodes[graph.edges[i].to], Theta, DoRotation),
                i == selectedEdge ? Color.Yellow : Color.White
            );
        }
    }
}