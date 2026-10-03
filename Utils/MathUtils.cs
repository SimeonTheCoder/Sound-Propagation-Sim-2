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

    public static Vector2 TransformVec(Vector2 vec, Vector2 i, Vector2 j)
    {
        return vec.X * i + vec.Y * j;
    }

    public static Vector2 RotateVec(Vector2 vec, float theta, bool should = true)
    {
        if (!should)
            return vec;
        return TransformVec(
            vec,
            new(MathF.Cos(theta / 180f * MathF.PI), MathF.Sin(theta / 180f * MathF.PI)),
            new(-MathF.Sin(theta / 180f * MathF.PI), MathF.Cos(theta / 180f * MathF.PI))
        );
    }

    public static Vector2 DoAxisFlip(Vector2 vec, float theta, float xo, float yo)
    {
        float thetaRad = theta / 180f * MathF.PI;
        Vector2 copy = new(vec.X, vec.Y);

        copy -= new Vector2(xo, yo);
        copy = TransformVec(
            copy,
            new(MathF.Cos(-thetaRad), MathF.Sin(-thetaRad)),
            new(-MathF.Sin(-thetaRad), MathF.Cos(-thetaRad))
        );

        copy = TransformVec(copy, new(1f, 0f), new(0f, -1f));

        copy = TransformVec(
            copy,
            new(MathF.Cos(thetaRad), MathF.Sin(thetaRad)),
            new(-MathF.Sin(thetaRad), MathF.Cos(thetaRad))
        );
        copy += new Vector2(xo, yo);

        return copy;
    }
}