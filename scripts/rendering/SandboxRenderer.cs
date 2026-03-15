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
    private IReadOnlyList<SurvivalRegion>? _survivalRegions;
    private HashSet<Agent> _selectedSurvivors = new();
    private bool _selectionActive;
    private float _selectionProgress;

    public void Bind(Simulation simulation, RunConfiguration runConfiguration)
    {
        _simulation = simulation;
        _runConfiguration = runConfiguration;
        QueueRedraw();
    }

    public void SetSurvivalRegions(IReadOnlyList<SurvivalRegion> survivalRegions)
    {
        _survivalRegions = survivalRegions;
        QueueRedraw();
    }

    public void SetSelectionState(IReadOnlyCollection<Agent> selectedSurvivors, bool selectionActive, float selectionProgress)
    {
        _selectedSurvivors = new HashSet<Agent>(selectedSurvivors);
        _selectionActive = selectionActive;
        _selectionProgress = selectionProgress;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_simulation is null || _runConfiguration is null)
        {
            return;
        }

        var worldRect = new Rect2(_runConfiguration.WorldOrigin, _simulation.WorldState.Size);

        DrawRect(
            worldRect,
            new Color(0.08f, 0.1f, 0.12f),
            filled: true);

        if (_selectionActive && _survivalRegions is not null)
        {
            foreach (var region in _survivalRegions)
            {
                DrawRect(
                    new Rect2(_runConfiguration.WorldOrigin + region.Bounds.Position, region.Bounds.Size),
                    new Color(0.32f, 0.56f, 0.36f, 0.10f + (0.22f * _selectionProgress)),
                    filled: true);
            }
        }

        DrawRect(worldRect, new Color(0.24f, 0.28f, 0.33f), filled: false, width: 2.0f);

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
}
