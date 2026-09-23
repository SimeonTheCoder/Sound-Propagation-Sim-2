using System.Numerics;

namespace Utils;

public class MathUtils
{
    public static Vector2 GetReflectedCoords(Vector2 vector, int x, int y, Vector2 spaceDimensions)
    {
        return new(
            x % 2 == 0 ? x * spaceDimensions.X + vector.X : (x + 1) * spaceDimensions.X - vector.X,
            y % 2 == 0 ? y * spaceDimensions.Y + vector.Y : (y + 1) * spaceDimensions.Y - vector.Y
        );
    }
}