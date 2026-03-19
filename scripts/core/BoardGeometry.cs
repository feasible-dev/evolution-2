using System;
using Godot;

namespace EvoPrototype.Core;

public static class BoardGeometry
{
    public static Rect2 GetBoundingSquare(Vector2 worldSize)
    {
        var side = Mathf.Min(worldSize.X, worldSize.Y);
        var offset = (worldSize - new Vector2(side, side)) * 0.5f;
        return new Rect2(offset, new Vector2(side, side));
    }

    public static bool ContainsPoint(Vector2 point, Vector2 worldSize, BoardShapeKind shape, float margin = 0.0f)
    {
        var square = GetBoundingSquare(worldSize);
        return shape switch
        {
            BoardShapeKind.Square => GetInsetRect(square, margin).HasPoint(point),
            BoardShapeKind.Circle => point.DistanceTo(square.GetCenter()) <= Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f),
            BoardShapeKind.Hexagon => IsInsidePolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => IsInsidePolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 8, (Mathf.Pi / 8.0f))),
            _ => false
        };
    }

    public static Vector2 ClampPoint(Vector2 point, Vector2 worldSize, BoardShapeKind shape, float margin = 0.0f)
    {
        if (ContainsPoint(point, worldSize, shape, margin))
        {
            return point;
        }

        var square = GetBoundingSquare(worldSize);
        return shape switch
        {
            BoardShapeKind.Square => ClampToRect(point, GetInsetRect(square, margin)),
            BoardShapeKind.Circle => ClampToCircle(point, square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f)),
            BoardShapeKind.Hexagon => ClampToPolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => ClampToPolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 8, (Mathf.Pi / 8.0f))),
            _ => point
        };
    }

    public static float DistanceToBoundary(Vector2 origin, Vector2 direction, Vector2 worldSize, BoardShapeKind shape)
    {
        var dir = direction.Normalized();
        if (dir.LengthSquared() <= 0.0001f)
        {
            return 0.0f;
        }

        var square = GetBoundingSquare(worldSize);
        return shape switch
        {
            BoardShapeKind.Square => RayToRect(origin, dir, square),
            BoardShapeKind.Circle => RayToCircle(origin, dir, square.GetCenter(), square.Size.X * 0.5f),
            BoardShapeKind.Hexagon => RayToPolygon(origin, dir, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => RayToPolygon(origin, dir, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 8, (Mathf.Pi / 8.0f))),
            _ => 0.0f
        };
    }

    public static Vector2 RandomPointInside(Random random, Vector2 worldSize, BoardShapeKind shape, float margin)
    {
        var square = GetBoundingSquare(worldSize);
        for (var attempt = 0; attempt < 128; attempt++)
        {
            var point = new Vector2(
                square.Position.X + ((float)random.NextDouble() * square.Size.X),
                square.Position.Y + ((float)random.NextDouble() * square.Size.Y));
            if (ContainsPoint(point, worldSize, shape, margin))
            {
                return point;
            }
        }

        return ClampPoint(square.GetCenter(), worldSize, shape, margin);
    }

    public static Vector2 GetBoundaryNormal(Vector2 point, Vector2 worldSize, BoardShapeKind shape)
    {
        var square = GetBoundingSquare(worldSize);
        return shape switch
        {
            BoardShapeKind.Square => RectBoundaryNormal(point, square),
            BoardShapeKind.Circle => (point - square.GetCenter()).Normalized(),
            BoardShapeKind.Hexagon => PolygonBoundaryNormal(point, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => PolygonBoundaryNormal(point, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 8, (Mathf.Pi / 8.0f))),
            _ => Vector2.Zero
        };
    }

    public static Vector2[] GetOutline(Vector2 worldSize, BoardShapeKind shape)
    {
        var square = GetBoundingSquare(worldSize);
        return shape switch
        {
            BoardShapeKind.Square => new[]
            {
                square.Position,
                square.Position + new Vector2(square.Size.X, 0.0f),
                square.End,
                square.Position + new Vector2(0.0f, square.Size.Y)
            },
            BoardShapeKind.Hexagon => GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 6, -Mathf.Pi * 0.5f),
            BoardShapeKind.Octagon => GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 8, (Mathf.Pi / 8.0f)),
            _ => Array.Empty<Vector2>()
        };
    }

    private static Rect2 GetInsetRect(Rect2 rect, float inset)
    {
        return new Rect2(
            rect.Position + new Vector2(inset, inset),
            new Vector2(Mathf.Max(rect.Size.X - (inset * 2.0f), 0.0f), Mathf.Max(rect.Size.Y - (inset * 2.0f), 0.0f)));
    }

    private static Vector2 ClampToRect(Vector2 point, Rect2 rect)
    {
        return new Vector2(
            Mathf.Clamp(point.X, rect.Position.X, rect.End.X),
            Mathf.Clamp(point.Y, rect.Position.Y, rect.End.Y));
    }

    private static Vector2 ClampToCircle(Vector2 point, Vector2 center, float radius)
    {
        var offset = point - center;
        if (offset.LengthSquared() <= 0.0001f)
        {
            return center + new Vector2(radius, 0.0f);
        }

        return center + offset.Normalized() * radius;
    }

    private static Vector2 ClampToPolygon(Vector2 point, Vector2[] polygon)
    {
        var bestPoint = polygon[0];
        var bestDistance = float.MaxValue;
        for (var index = 0; index < polygon.Length; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Length];
            var closest = Geometry2D.GetClosestPointToSegment(point, start, end);
            var distance = point.DistanceSquaredTo(closest);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPoint = closest;
            }
        }

        return bestPoint;
    }

    private static bool IsInsidePolygon(Vector2 point, Vector2[] polygon)
    {
        return Geometry2D.IsPointInPolygon(point, polygon);
    }

    private static float RayToRect(Vector2 origin, Vector2 direction, Rect2 rect)
    {
        const float epsilon = 0.0001f;
        var minDistance = float.MaxValue;

        if (Mathf.Abs(direction.X) > epsilon)
        {
            var tx1 = (rect.Position.X - origin.X) / direction.X;
            var tx2 = (rect.End.X - origin.X) / direction.X;
            if (tx1 >= 0.0f)
            {
                var y = origin.Y + (direction.Y * tx1);
                if (y >= rect.Position.Y - epsilon && y <= rect.End.Y + epsilon)
                {
                    minDistance = Mathf.Min(minDistance, tx1);
                }
            }

            if (tx2 >= 0.0f)
            {
                var y = origin.Y + (direction.Y * tx2);
                if (y >= rect.Position.Y - epsilon && y <= rect.End.Y + epsilon)
                {
                    minDistance = Mathf.Min(minDistance, tx2);
                }
            }
        }

        if (Mathf.Abs(direction.Y) > epsilon)
        {
            var ty1 = (rect.Position.Y - origin.Y) / direction.Y;
            var ty2 = (rect.End.Y - origin.Y) / direction.Y;
            if (ty1 >= 0.0f)
            {
                var x = origin.X + (direction.X * ty1);
                if (x >= rect.Position.X - epsilon && x <= rect.End.X + epsilon)
                {
                    minDistance = Mathf.Min(minDistance, ty1);
                }
            }

            if (ty2 >= 0.0f)
            {
                var x = origin.X + (direction.X * ty2);
                if (x >= rect.Position.X - epsilon && x <= rect.End.X + epsilon)
                {
                    minDistance = Mathf.Min(minDistance, ty2);
                }
            }
        }

        return minDistance == float.MaxValue ? 0.0f : minDistance;
    }

    private static float RayToCircle(Vector2 origin, Vector2 direction, Vector2 center, float radius)
    {
        var offset = origin - center;
        var a = direction.Dot(direction);
        var b = 2.0f * offset.Dot(direction);
        var c = offset.Dot(offset) - (radius * radius);
        var discriminant = (b * b) - (4.0f * a * c);
        if (discriminant < 0.0f)
        {
            return 0.0f;
        }

        var sqrt = Mathf.Sqrt(discriminant);
        var t1 = (-b - sqrt) / (2.0f * a);
        var t2 = (-b + sqrt) / (2.0f * a);
        var best = float.MaxValue;
        if (t1 >= 0.0f)
        {
            best = Mathf.Min(best, t1);
        }
        if (t2 >= 0.0f)
        {
            best = Mathf.Min(best, t2);
        }

        return best == float.MaxValue ? 0.0f : best;
    }

    private static float RayToPolygon(Vector2 origin, Vector2 direction, Vector2[] polygon)
    {
        var best = float.MaxValue;
        for (var index = 0; index < polygon.Length; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Length];
            if (RaySegmentIntersection(origin, direction, start, end, out var distance) && distance >= 0.0f)
            {
                best = Mathf.Min(best, distance);
            }
        }

        return best == float.MaxValue ? 0.0f : best;
    }

    private static bool RaySegmentIntersection(Vector2 origin, Vector2 direction, Vector2 a, Vector2 b, out float distance)
    {
        var segment = b - a;
        var denominator = Cross(direction, segment);
        if (Mathf.Abs(denominator) < 0.0001f)
        {
            distance = 0.0f;
            return false;
        }

        var diff = a - origin;
        var t = Cross(diff, segment) / denominator;
        var u = Cross(diff, direction) / denominator;
        distance = t;
        return t >= 0.0f && u >= 0.0f && u <= 1.0f;
    }

    private static float Cross(Vector2 left, Vector2 right)
    {
        return (left.X * right.Y) - (left.Y * right.X);
    }

    private static Vector2 RectBoundaryNormal(Vector2 point, Rect2 rect)
    {
        var distances = new[]
        {
            (Mathf.Abs(point.X - rect.Position.X), new Vector2(-1.0f, 0.0f)),
            (Mathf.Abs(point.X - rect.End.X), new Vector2(1.0f, 0.0f)),
            (Mathf.Abs(point.Y - rect.Position.Y), new Vector2(0.0f, -1.0f)),
            (Mathf.Abs(point.Y - rect.End.Y), new Vector2(0.0f, 1.0f))
        };

        var best = distances[0];
        for (var index = 1; index < distances.Length; index++)
        {
            if (distances[index].Item1 < best.Item1)
            {
                best = distances[index];
            }
        }

        return best.Item2;
    }

    private static Vector2 PolygonBoundaryNormal(Vector2 point, Vector2[] polygon)
    {
        var bestNormal = Vector2.Up;
        var bestDistance = float.MaxValue;
        for (var index = 0; index < polygon.Length; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Length];
            var closest = Geometry2D.GetClosestPointToSegment(point, start, end);
            var distance = point.DistanceSquaredTo(closest);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                var edge = end - start;
                bestNormal = edge.Orthogonal().Normalized();
            }
        }

        var center = Vector2.Zero;
        foreach (var vertex in polygon)
        {
            center += vertex;
        }

        center /= polygon.Length;
        if ((point - center).Dot(bestNormal) < 0.0f)
        {
            bestNormal = -bestNormal;
        }

        return bestNormal;
    }

    private static Vector2[] GetRegularPolygon(Vector2 center, float radius, int sideCount, float angleOffset)
    {
        var vertices = new Vector2[sideCount];
        for (var index = 0; index < sideCount; index++)
        {
            var angle = angleOffset + ((Mathf.Tau * index) / sideCount);
            vertices[index] = center + Vector2.FromAngle(angle) * radius;
        }

        return vertices;
    }
}
