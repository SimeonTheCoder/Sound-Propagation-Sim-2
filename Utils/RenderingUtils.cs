using System.Numerics;
using Raylib_cs;

using static Shared;

namespace Utils;

public class RenderingUtils
{
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
        Raylib.DrawRectangle(x - (int) size.X / 2, y - (int) size.Y / 2, (int) size.X, (int) size.Y, color);
    }
}