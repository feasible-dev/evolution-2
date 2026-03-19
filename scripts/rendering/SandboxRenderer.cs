using System.Collections.Generic;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using EvoPrototype.Game;
using Godot;

namespace EvoPrototype.Rendering;

public partial class SandboxRenderer : Node2D
{
    private Simulation? _simulation;
    private RunConfiguration? _runConfiguration;
    private IReadOnlyList<ISurvivalZone>? _survivalZones;
    private HashSet<Agent> _selectedSurvivors = new();
    private bool _selectionActive;
    private float _selectionProgress;
    private readonly Dictionary<Agent, Queue<Vector2>> _trailHistory = new();
    private bool _liveRenderingEnabled = true;

    public void Bind(Simulation simulation, RunConfiguration runConfiguration)
    {
        _simulation = simulation;
        _runConfiguration = runConfiguration;
        _trailHistory.Clear();
        QueueRedraw();
    }

    public void SetSurvivalZones(IReadOnlyList<ISurvivalZone> survivalZones)
    {
        _survivalZones = survivalZones;
        QueueRedraw();
    }

    public void SetSelectionState(IReadOnlyCollection<Agent> selectedSurvivors, bool selectionActive, float selectionProgress)
    {
        _selectedSurvivors = new HashSet<Agent>(selectedSurvivors);
        _selectionActive = selectionActive;
        _selectionProgress = selectionProgress;
        QueueRedraw();
    }

    public void SetLiveRenderingEnabled(bool enabled)
    {
        _liveRenderingEnabled = enabled;
        QueueRedraw();
    }

    public void CaptureFrame()
    {
        if (_simulation is null || _runConfiguration is null)
        {
            return;
        }

        var activeAgents = new HashSet<Agent>(_simulation.WorldState.Agents);
        foreach (var key in new List<Agent>(_trailHistory.Keys))
        {
            if (!activeAgents.Contains(key))
            {
                _trailHistory.Remove(key);
            }
        }

        foreach (var agent in _simulation.WorldState.Agents)
        {
            if (!_trailHistory.TryGetValue(agent, out var history))
            {
                history = new Queue<Vector2>();
                _trailHistory[agent] = history;
            }

            history.Enqueue(agent.Position);
            while (history.Count > _runConfiguration.TrailPointCount)
            {
                history.Dequeue();
            }
        }
    }

    public override void _Draw()
    {
        if (_simulation is null || _runConfiguration is null)
        {
            return;
        }

        var worldRect = new Rect2(_runConfiguration.WorldOrigin, _simulation.WorldState.Size);
        DrawRect(worldRect, new Color(0.24f, 0.24f, 0.24f), filled: true);
        DrawBoardSurface();
        DrawObstacles();

        if (_liveRenderingEnabled)
        {
            DrawTrails();
        }

        if (_selectionActive && _survivalZones is not null)
        {
            foreach (var zone in _survivalZones)
            {
                DrawSurvivalZone(zone);
            }
        }

        DrawBoardOutline();

        foreach (var agent in _simulation.WorldState.Agents)
        {
            var drawColor = agent.Color;
            var drawPosition = _runConfiguration.WorldOrigin + agent.Position;

            if (_selectionActive)
            {
                var isSurvivor = _selectedSurvivors.Contains(agent);
                drawColor = isSurvivor
                    ? agent.Color.Lerp(Colors.White, 0.2f + (0.35f * _selectionProgress))
                    : agent.Color.Lerp(new Color(0.45f, 0.08f, 0.08f), 0.65f);

                DrawCircle(drawPosition, agent.Radius, drawColor);

                if (!isSurvivor)
                {
                    DrawDeathMarker(drawPosition, agent.Radius, _selectionProgress);
                }

                continue;
            }

            DrawCircle(drawPosition, agent.Radius, drawColor);
        }
    }

    private void DrawDeathMarker(Vector2 center, float radius, float progress)
    {
        var markerRadius = radius + 2.0f + progress;
        var color = new Color(1.0f, 0.9f, 0.9f, 0.45f + (0.45f * progress));
        var offset = new Vector2(markerRadius, markerRadius);

        DrawLine(center - offset, center + offset, color, width: 1.6f);
        DrawLine(
            new Vector2(center.X - markerRadius, center.Y + markerRadius),
            new Vector2(center.X + markerRadius, center.Y - markerRadius),
            color,
            width: 1.6f);
    }

    private void DrawSurvivalZone(ISurvivalZone zone)
    {
        var fillColor = new Color(0.32f, 0.56f, 0.36f, 0.10f + (0.22f * _selectionProgress));

        switch (zone)
        {
            case RectangleSurvivalZone rectangleZone:
                DrawRect(
                    new Rect2(_runConfiguration!.WorldOrigin + rectangleZone.Bounds.Position, rectangleZone.Bounds.Size),
                    fillColor,
                    filled: true);
                break;

            case CircleSurvivalZone circleZone:
                DrawCircle(_runConfiguration!.WorldOrigin + circleZone.Center, circleZone.Radius, fillColor);
                break;
        }
    }

    private void DrawTrails()
    {
        foreach (var (agent, history) in _trailHistory)
        {
            if (history.Count < 2)
            {
                continue;
            }

            var points = history.ToArray();
            for (var index = 1; index < points.Length; index++)
            {
                var alpha = _runConfiguration!.TrailAlpha * (index / (float)points.Length);
                var color = agent.Color with { A = alpha };
                DrawLine(
                    _runConfiguration.WorldOrigin + points[index - 1],
                    _runConfiguration.WorldOrigin + points[index],
                    color,
                    width: 1.0f);
            }
        }
    }

    private void DrawBoardSurface()
    {
        var shape = _runConfiguration!.BoardShape;
        var worldSize = _simulation!.WorldState.Size;
        var origin = _runConfiguration.WorldOrigin;
        var fill = new Color(0.08f, 0.10f, 0.12f);
        var square = BoardGeometry.GetBoundingSquare(worldSize);

        switch (shape)
        {
            case BoardShapeKind.Square:
                DrawRect(new Rect2(origin + square.Position, square.Size), fill, filled: true);
                break;
            case BoardShapeKind.Circle:
                DrawCircle(origin + square.GetCenter(), square.Size.X * 0.5f, fill);
                break;
            case BoardShapeKind.Hexagon:
            case BoardShapeKind.Octagon:
                var polygon = BoardGeometry.GetOutline(worldSize, shape);
                var translated = new Vector2[polygon.Length];
                for (var index = 0; index < polygon.Length; index++)
                {
                    translated[index] = origin + polygon[index];
                }
                DrawColoredPolygon(translated, fill);
                break;
        }
    }

    private void DrawBoardOutline()
    {
        var shape = _runConfiguration!.BoardShape;
        var worldSize = _simulation!.WorldState.Size;
        var origin = _runConfiguration.WorldOrigin;
        var outline = new Color(0.24f, 0.28f, 0.33f);
        var square = BoardGeometry.GetBoundingSquare(worldSize);

        switch (shape)
        {
            case BoardShapeKind.Square:
                DrawRect(new Rect2(origin + square.Position, square.Size), outline, filled: false, width: 2.0f);
                break;
            case BoardShapeKind.Circle:
                DrawArc(origin + square.GetCenter(), square.Size.X * 0.5f, 0.0f, Mathf.Tau, 96, outline, 2.0f);
                break;
            case BoardShapeKind.Hexagon:
            case BoardShapeKind.Octagon:
                var polygon = BoardGeometry.GetOutline(worldSize, shape);
                for (var index = 0; index < polygon.Length; index++)
                {
                    var start = origin + polygon[index];
                    var end = origin + polygon[(index + 1) % polygon.Length];
                    DrawLine(start, end, outline, 2.0f);
                }
                break;
        }
    }

    private void DrawObstacles()
    {
        var obstacles = ObstacleFactory.Create(_runConfiguration!.WorldSize, _runConfiguration.ObstacleLayout);
        if (obstacles.Count == 0)
        {
            return;
        }

        var fill = new Color(0.42f, 0.46f, 0.54f, 0.72f);
        var outline = new Color(0.64f, 0.68f, 0.76f, 0.95f);
        foreach (var obstacle in obstacles)
        {
            switch (obstacle.Kind)
            {
                case BoardObstacleKind.Circle:
                    DrawCircle(_runConfiguration.WorldOrigin + obstacle.Center, obstacle.Radius, fill);
                    DrawArc(_runConfiguration.WorldOrigin + obstacle.Center, obstacle.Radius, 0.0f, Mathf.Tau, 48, outline, 2.0f);
                    break;
                case BoardObstacleKind.Rectangle:
                    var rect = new Rect2(
                        _runConfiguration.WorldOrigin + obstacle.Center - (obstacle.Size * 0.5f),
                        obstacle.Size);
                    DrawRect(rect, fill, filled: true);
                    DrawRect(rect, outline, filled: false, width: 2.0f);
                    break;
            }
        }
    }
}
