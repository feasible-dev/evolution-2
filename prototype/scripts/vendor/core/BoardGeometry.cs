using System;
using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

public static class BoardGeometry
{
    public const float RoundedRectangleCornerRadiusFraction = 0.14f;
    // Local v04 optimization: shared immutable outlines avoid per-agent trigonometry/allocations.
    [ThreadStatic]
    private static Dictionary<(Rect2, float, int), Vector2[]>? _roundedPolygons;

    public static Rect2 GetBoundingSquare(Vector2 worldSize)
    {
        var side = Mathf.Min(worldSize.X, worldSize.Y);
        var offset = (worldSize - new Vector2(side, side)) * 0.5f;
        return new Rect2(offset, new Vector2(side, side));
    }

    public static bool ContainsPoint(Vector2 point, Vector2 worldSize, BoardShapeKind shape, float margin = 0.0f)
    {
        var square = GetBoundingSquare(worldSize);
        var boardRect = GetBoardRect(worldSize, shape);
        return shape switch
        {
            BoardShapeKind.Square => GetInsetRect(square, margin).HasPoint(point),
            BoardShapeKind.Circle => point.DistanceTo(square.GetCenter()) <= Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f),
            BoardShapeKind.Hexagon => IsInsidePolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => IsInsidePolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 8, (Mathf.Pi / 8.0f))),
            BoardShapeKind.RoundedRectangle => ContainsRounded(point, GetInsetRect(boardRect, margin), GetRoundedCornerRadius(boardRect, margin)),
            _ => false
        };
    }

    public static bool IsInsidePlayableArea(
        Vector2 point,
        Vector2 worldSize,
        BoardShapeKind shape,
        IReadOnlyList<BoardObstacle> obstacles,
        float margin = 0.0f)
    {
        if (!ContainsPoint(point, worldSize, shape, margin))
        {
            return false;
        }

        foreach (var obstacle in obstacles)
        {
            if (ContainsObstacle(point, obstacle, margin))
            {
                return false;
            }
        }

        return true;
    }

    public static Vector2 ClampPoint(Vector2 point, Vector2 worldSize, BoardShapeKind shape, float margin = 0.0f)
    {
        if (ContainsPoint(point, worldSize, shape, margin))
        {
            return point;
        }

        var square = GetBoundingSquare(worldSize);
        var boardRect = GetBoardRect(worldSize, shape);
        return shape switch
        {
            BoardShapeKind.Square => ClampToRect(point, GetInsetRect(square, margin)),
            BoardShapeKind.Circle => ClampToCircle(point, square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f)),
            BoardShapeKind.Hexagon => ClampToPolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => ClampToPolygon(point, GetRegularPolygon(square.GetCenter(), Mathf.Max((square.Size.X * 0.5f) - margin, 0.0f), 8, (Mathf.Pi / 8.0f))),
            BoardShapeKind.RoundedRectangle => ClampRounded(point, GetInsetRect(boardRect, margin), GetRoundedCornerRadius(boardRect, margin)),
            _ => point
        };
    }

    public static Vector2 ClampPoint(
        Vector2 point,
        Vector2 worldSize,
        BoardShapeKind shape,
        IReadOnlyList<BoardObstacle> obstacles,
        float margin = 0.0f)
    {
        var clamped = ClampPoint(point, worldSize, shape, margin);

        foreach (var obstacle in obstacles)
        {
            if (ContainsObstacle(clamped, obstacle, margin))
            {
                clamped = ClampOutsideObstacle(clamped, obstacle, margin);
            }
        }

        return ClampPoint(clamped, worldSize, shape, margin);
    }

    public static float DistanceToBoundary(Vector2 origin, Vector2 direction, Vector2 worldSize, BoardShapeKind shape)
    {
        var dir = direction.Normalized();
        if (dir.LengthSquared() <= 0.0001f)
        {
            return 0.0f;
        }

        var square = GetBoundingSquare(worldSize);
        var boardRect = GetBoardRect(worldSize, shape);
        return shape switch
        {
            BoardShapeKind.Square => RayToRect(origin, dir, square),
            BoardShapeKind.Circle => RayToCircle(origin, dir, square.GetCenter(), square.Size.X * 0.5f),
            BoardShapeKind.Hexagon => RayToPolygon(origin, dir, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => RayToPolygon(origin, dir, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 8, (Mathf.Pi / 8.0f))),
            BoardShapeKind.RoundedRectangle => RayToPolygon(origin, dir, GetRoundedRectPolygon(boardRect, GetRoundedCornerRadius(boardRect, 0.0f))),
            _ => 0.0f
        };
    }

    public static float DistanceToBoundary(
        Vector2 origin,
        Vector2 direction,
        Vector2 worldSize,
        BoardShapeKind shape,
        IReadOnlyList<BoardObstacle> obstacles)
    {
        var best = DistanceToBoundary(origin, direction, worldSize, shape);

        foreach (var obstacle in obstacles)
        {
            var obstacleDistance = RayToObstacle(origin, direction, obstacle);
            if (obstacleDistance > 0.0f)
            {
                best = best <= 0.0f ? obstacleDistance : Mathf.Min(best, obstacleDistance);
            }
        }

        return best;
    }

    public static Vector2 RandomPointInside(Random random, Vector2 worldSize, BoardShapeKind shape, float margin)
    {
        var boardRect = GetBoardRect(worldSize, shape);
        for (var attempt = 0; attempt < 128; attempt++)
        {
            var point = new Vector2(
                boardRect.Position.X + ((float)random.NextDouble() * boardRect.Size.X),
                boardRect.Position.Y + ((float)random.NextDouble() * boardRect.Size.Y));
            if (ContainsPoint(point, worldSize, shape, margin))
            {
                return point;
            }
        }

        return ClampPoint(boardRect.GetCenter(), worldSize, shape, margin);
    }

    public static Vector2 RandomPointInside(
        Random random,
        Vector2 worldSize,
        BoardShapeKind shape,
        IReadOnlyList<BoardObstacle> obstacles,
        float margin)
    {
        var boardRect = GetBoardRect(worldSize, shape);
        for (var attempt = 0; attempt < 192; attempt++)
        {
            var point = new Vector2(
                boardRect.Position.X + ((float)random.NextDouble() * boardRect.Size.X),
                boardRect.Position.Y + ((float)random.NextDouble() * boardRect.Size.Y));
            if (IsInsidePlayableArea(point, worldSize, shape, obstacles, margin))
            {
                return point;
            }
        }

        return ClampPoint(boardRect.GetCenter(), worldSize, shape, obstacles, margin);
    }

    public static Vector2 GetBoundaryNormal(Vector2 point, Vector2 worldSize, BoardShapeKind shape)
    {
        var square = GetBoundingSquare(worldSize);
        var boardRect = GetBoardRect(worldSize, shape);
        return shape switch
        {
            BoardShapeKind.Square => RectBoundaryNormal(point, square),
            BoardShapeKind.Circle => (point - square.GetCenter()).Normalized(),
            BoardShapeKind.Hexagon => PolygonBoundaryNormal(point, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 6, -Mathf.Pi * 0.5f)),
            BoardShapeKind.Octagon => PolygonBoundaryNormal(point, GetRegularPolygon(square.GetCenter(), square.Size.X * 0.5f, 8, (Mathf.Pi / 8.0f))),
            BoardShapeKind.RoundedRectangle => RoundedBoundaryNormal(point, boardRect, GetRoundedCornerRadius(boardRect, 0.0f)),
            _ => Vector2.Zero
        };
    }

    public static bool TryGetObstacleBoundaryNormal(
        Vector2 point,
        IReadOnlyList<BoardObstacle> obstacles,
        float margin,
        out Vector2 normal)
    {
        foreach (var obstacle in obstacles)
        {
            if (ContainsObstacle(point, obstacle, margin))
            {
                normal = GetObstacleBoundaryNormal(point, obstacle);
                return true;
            }
        }

        normal = Vector2.Zero;
        return false;
    }

    public static Vector2[] GetOutline(Vector2 worldSize, BoardShapeKind shape)
    {
        var square = GetBoundingSquare(worldSize);
        var boardRect = GetBoardRect(worldSize, shape);
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
            BoardShapeKind.RoundedRectangle => (Vector2[])GetRoundedRectPolygon(boardRect, GetRoundedCornerRadius(boardRect, 0.0f)).Clone(),
            _ => Array.Empty<Vector2>()
        };
    }

    private static Rect2 GetBoardRect(Vector2 worldSize, BoardShapeKind shape)
    {
        return shape == BoardShapeKind.RoundedRectangle
            ? new Rect2(Vector2.Zero, worldSize)
            : GetBoundingSquare(worldSize);
    }

    private static float GetRoundedCornerRadius(Rect2 rect, float margin)
    {
        var radius = Mathf.Min(rect.Size.X, rect.Size.Y) * RoundedRectangleCornerRadiusFraction;
        return Mathf.Max(radius - margin, 0.0f);
    }

    private static Vector2[] GetRoundedRectPolygon(Rect2 rect, float radius, int arcSegments = 10)
    {
        var cache = _roundedPolygons ??= new();
        var key = (rect, radius, arcSegments);
        if (cache.TryGetValue(key, out var outline)) return outline;
        if (cache.Count >= 64) cache.Clear();
        outline = BuildRoundedRectPolygon(rect, radius, arcSegments);
        cache[key] = outline;
        return outline;
    }

    private static bool InStraightStrip(Vector2 point, Rect2 rect, float radius)
    {
        // Keep transition endpoints in the original polygon path to preserve normal tie breaking.
        return (point.X > rect.Position.X + radius && point.X < rect.End.X - radius)
            || (point.Y > rect.Position.Y + radius && point.Y < rect.End.Y - radius);
    }

    private static bool ContainsRounded(Vector2 point, Rect2 rect, float radius)
    {
        if (point.X < rect.Position.X || point.X > rect.End.X || point.Y < rect.Position.Y || point.Y > rect.End.Y) return false;
        return InStraightStrip(point, rect, radius) || IsInsidePolygon(point, GetRoundedRectPolygon(rect, radius));
    }

    private static Vector2 ClampRounded(Vector2 point, Rect2 rect, float radius)
    {
        if (InStraightStrip(point, rect, radius)) return ClampToRect(point, rect);
        return ClampToPolygon(point, GetRoundedRectPolygon(rect, radius));
    }

    private static Vector2 RoundedBoundaryNormal(Vector2 point, Rect2 rect, float radius)
    {
        var onStraightBoundary = point.X == rect.Position.X || point.X == rect.End.X
            || point.Y == rect.Position.Y || point.Y == rect.End.Y;
        if (onStraightBoundary && InStraightStrip(point, rect, radius)) return RectBoundaryNormal(point, rect);
        return PolygonBoundaryNormal(point, GetRoundedRectPolygon(rect, radius));
    }

    private static Vector2[] BuildRoundedRectPolygon(Rect2 rect, float radius, int arcSegments)
    {
        radius = Mathf.Clamp(radius, 0.0f, Mathf.Min(rect.Size.X, rect.Size.Y) * 0.5f);
        if (radius <= 0.0f)
        {
            return new[]
            {
                rect.Position,
                new Vector2(rect.End.X, rect.Position.Y),
                rect.End,
                new Vector2(rect.Position.X, rect.End.Y)
            };
        }

        var points = new List<Vector2>(arcSegments * 4);
        AppendArc(points, new Vector2(rect.End.X - radius, rect.Position.Y + radius), -Mathf.Pi * 0.5f, 0.0f, radius, arcSegments);
        AppendArc(points, new Vector2(rect.End.X - radius, rect.End.Y - radius), 0.0f, Mathf.Pi * 0.5f, radius, arcSegments);
        AppendArc(points, new Vector2(rect.Position.X + radius, rect.End.Y - radius), Mathf.Pi * 0.5f, Mathf.Pi, radius, arcSegments);
        AppendArc(points, new Vector2(rect.Position.X + radius, rect.Position.Y + radius), Mathf.Pi, Mathf.Pi * 1.5f, radius, arcSegments);
        return points.ToArray();
    }

    private static void AppendArc(List<Vector2> points, Vector2 center, float startAngle, float endAngle, float radius, int segments)
    {
        for (var index = 0; index <= segments; index++)
        {
            var t = index / (float)segments;
            var angle = Mathf.Lerp(startAngle, endAngle, t);
            var point = center + Vector2.FromAngle(angle) * radius;
            if (points.Count == 0 || points[^1].DistanceSquaredTo(point) > 0.0001f)
            {
                points.Add(point);
            }
        }
    }

    public static bool ContainsObstacle(Vector2 point, BoardObstacle obstacle, float margin = 0.0f)
    {
        return obstacle.Kind switch
        {
            BoardObstacleKind.Circle => point.DistanceTo(obstacle.Center) <= obstacle.Radius + margin,
            BoardObstacleKind.Rectangle => GetExpandedRect(obstacle, margin).HasPoint(point),
            _ => false
        };
    }

    public static Vector2 ClampOutsideObstacle(Vector2 point, BoardObstacle obstacle, float margin = 0.0f)
    {
        return obstacle.Kind switch
        {
            BoardObstacleKind.Circle => ClampOutsideCircle(point, obstacle.Center, obstacle.Radius + margin),
            BoardObstacleKind.Rectangle => ClampOutsideRect(point, GetExpandedRect(obstacle, margin)),
            _ => point
        };
    }

    public static Vector2 GetObstacleBoundaryNormal(Vector2 point, BoardObstacle obstacle)
    {
        return obstacle.Kind switch
        {
            BoardObstacleKind.Circle => (point - obstacle.Center).LengthSquared() > 0.0001f
                ? (point - obstacle.Center).Normalized()
                : Vector2.Right,
            BoardObstacleKind.Rectangle => RectObstacleNormal(point, obstacle),
            _ => Vector2.Zero
        };
    }

    private static Rect2 GetInsetRect(Rect2 rect, float inset)
    {
        return new Rect2(
            rect.Position + new Vector2(inset, inset),
            new Vector2(Mathf.Max(rect.Size.X - (inset * 2.0f), 0.0f), Mathf.Max(rect.Size.Y - (inset * 2.0f), 0.0f)));
    }

    private static Rect2 GetExpandedRect(BoardObstacle obstacle, float margin)
    {
        var half = (obstacle.Size * 0.5f) + new Vector2(margin, margin);
        return new Rect2(obstacle.Center - half, half * 2.0f);
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

    private static Vector2 ClampOutsideCircle(Vector2 point, Vector2 center, float radius)
    {
        var offset = point - center;
        if (offset.LengthSquared() <= 0.0001f)
        {
            return center + new Vector2(radius, 0.0f);
        }

        return center + offset.Normalized() * radius;
    }

    private static Vector2 ClampOutsideRect(Vector2 point, Rect2 rect)
    {
        var left = Mathf.Abs(point.X - rect.Position.X);
        var right = Mathf.Abs(rect.End.X - point.X);
        var top = Mathf.Abs(point.Y - rect.Position.Y);
        var bottom = Mathf.Abs(rect.End.Y - point.Y);

        var minDistance = left;
        var result = new Vector2(rect.Position.X, point.Y);

        if (right < minDistance)
        {
            minDistance = right;
            result = new Vector2(rect.End.X, point.Y);
        }

        if (top < minDistance)
        {
            minDistance = top;
            result = new Vector2(point.X, rect.Position.Y);
        }

        if (bottom < minDistance)
        {
            result = new Vector2(point.X, rect.End.Y);
        }

        return result;
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

    private static float RayToObstacle(Vector2 origin, Vector2 direction, BoardObstacle obstacle)
    {
        var normalizedDirection = direction.Normalized();
        if (normalizedDirection.LengthSquared() <= 0.0001f)
        {
            return 0.0f;
        }

        return obstacle.Kind switch
        {
            BoardObstacleKind.Circle => RayToCircle(origin, normalizedDirection, obstacle.Center, obstacle.Radius),
            BoardObstacleKind.Rectangle => RayToRect(origin, normalizedDirection, GetExpandedRect(obstacle, 0.0f)),
            _ => 0.0f
        };
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

    private static Vector2 RectObstacleNormal(Vector2 point, BoardObstacle obstacle)
    {
        var rect = GetExpandedRect(obstacle, 0.0f);
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
