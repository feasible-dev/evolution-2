using System;
using EvoPrototype.Evolution;
using EvoPrototype.Game;
using Godot;

namespace EvoPrototype.Setup;

/// <summary>
/// Lets the player place and resize the current survival zone before starting a run.
/// </summary>
public partial class SetupOverlay : Node2D
{
    private const float HandleRadius = 8.0f;
    private const float EdgeSelectionTolerance = 12.0f;
    private RunConfiguration? _runConfiguration;
    private SurvivalZoneDefinition _definition = SurvivalZoneDefinition.Default;
    private bool _isEditingEnabled;
    private bool _draggingCenter;
    private bool _draggingRadius;

    public event Action<SurvivalZoneDefinition>? DefinitionChanged;

    public void Bind(RunConfiguration runConfiguration)
    {
        _runConfiguration = runConfiguration;
        QueueRedraw();
    }

    public void SetEditingEnabled(bool enabled)
    {
        _isEditingEnabled = enabled;
        QueueRedraw();
    }

    public void SetDefinition(SurvivalZoneDefinition definition)
    {
        _definition = definition;
        QueueRedraw();
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isEditingEnabled || _runConfiguration is null || _definition.Layout != SurvivalZoneLayout.CenterCircle)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                var worldPosition = ToWorldPosition(mouseButton.Position);
                if (!IsInsideWorld(worldPosition))
                {
                    return;
                }

                var circle = GetCircle();
                var distanceToCenter = circle.Center.DistanceTo(worldPosition);
                var distanceToEdge = MathF.Abs(distanceToCenter - circle.Radius);

                if (distanceToEdge <= EdgeSelectionTolerance)
                {
                    _draggingRadius = true;
                    GetViewport().SetInputAsHandled();
                }
                else if (distanceToCenter <= circle.Radius)
                {
                    _draggingCenter = true;
                    GetViewport().SetInputAsHandled();
                }
            }
            else
            {
                _draggingCenter = false;
                _draggingRadius = false;
            }
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (!_draggingCenter && !_draggingRadius)
            {
                return;
            }

            var worldPosition = ToWorldPosition(mouseMotion.Position);
            var clampedWorld = new Vector2(
                Mathf.Clamp(worldPosition.X, 0.0f, _runConfiguration.WorldSize.X),
                Mathf.Clamp(worldPosition.Y, 0.0f, _runConfiguration.WorldSize.Y));

            if (_draggingCenter)
            {
                UpdateCenter(clampedWorld);
            }
            else if (_draggingRadius)
            {
                UpdateRadius(clampedWorld);
            }

            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Draw()
    {
        if (_runConfiguration is null)
        {
            return;
        }

        var worldRect = new Rect2(_runConfiguration.WorldOrigin, _runConfiguration.WorldSize);
        DrawRect(worldRect, new Color(0.08f, 0.10f, 0.12f), filled: true);
        DrawRect(worldRect, new Color(0.24f, 0.28f, 0.33f), filled: false, width: 2.0f);

        if (_definition.Layout != SurvivalZoneLayout.CenterCircle)
        {
            return;
        }

        var circle = GetCircle();
        var drawCenter = _runConfiguration.WorldOrigin + circle.Center;
        var fillColor = _isEditingEnabled
            ? new Color(0.28f, 0.64f, 0.48f, 0.20f)
            : new Color(0.28f, 0.64f, 0.48f, 0.10f);
        var outlineColor = _isEditingEnabled
            ? new Color(0.72f, 0.94f, 0.84f)
            : new Color(0.52f, 0.76f, 0.68f);

        DrawCircle(drawCenter, circle.Radius, fillColor);
        DrawArc(drawCenter, circle.Radius, 0.0f, Mathf.Tau, 72, outlineColor, width: 2.0f);

        if (_isEditingEnabled)
        {
            DrawCircle(drawCenter, HandleRadius, new Color(0.94f, 0.98f, 0.92f, 0.95f));
            DrawCircle(
                drawCenter + new Vector2(circle.Radius, 0.0f),
                HandleRadius,
                new Color(0.98f, 0.86f, 0.52f, 0.95f));
        }
    }

    private (Vector2 Center, float Radius) GetCircle()
    {
        var center = new Vector2(
            _definition.CircleCenterNormalized.X * _runConfiguration!.WorldSize.X,
            _definition.CircleCenterNormalized.Y * _runConfiguration.WorldSize.Y);
        var radius = Mathf.Min(_runConfiguration.WorldSize.X, _runConfiguration.WorldSize.Y) * _definition.CircleRadiusFraction;
        return (center, radius);
    }

    private void UpdateCenter(Vector2 worldPosition)
    {
        var normalizedCenter = new Vector2(
            Mathf.Clamp(worldPosition.X / _runConfiguration!.WorldSize.X, 0.0f, 1.0f),
            Mathf.Clamp(worldPosition.Y / _runConfiguration.WorldSize.Y, 0.0f, 1.0f));
        ApplyDefinition(_definition with { CircleCenterNormalized = normalizedCenter });
    }

    private void UpdateRadius(Vector2 worldPosition)
    {
        var circle = GetCircle();
        var minSide = Mathf.Min(_runConfiguration!.WorldSize.X, _runConfiguration.WorldSize.Y);
        var radiusFraction = Mathf.Clamp(circle.Center.DistanceTo(worldPosition) / minSide, 0.04f, 0.48f);
        ApplyDefinition(_definition with { CircleRadiusFraction = radiusFraction });
    }

    private void ApplyDefinition(SurvivalZoneDefinition definition)
    {
        _definition = definition;
        DefinitionChanged?.Invoke(_definition);
        QueueRedraw();
    }

    private Vector2 ToWorldPosition(Vector2 viewportPosition)
    {
        return viewportPosition - _runConfiguration!.WorldOrigin;
    }

    private bool IsInsideWorld(Vector2 worldPosition)
    {
        return worldPosition.X >= 0.0f &&
               worldPosition.Y >= 0.0f &&
               worldPosition.X <= _runConfiguration!.WorldSize.X &&
               worldPosition.Y <= _runConfiguration.WorldSize.Y;
    }
}
