using EvoPrototype.Analytics;
using EvoPrototype.Game;
using Godot;

namespace EvoPrototype.Ui;

public partial class GenerationChart : Control
{
    private RunHistory? _runHistory;
    private MetricId _metricId = MetricId.DeathCount;

    public void Bind(RunHistory runHistory)
    {
        _runHistory = runHistory;
        QueueRedraw();
    }

    public void SetMetric(MetricId metricId)
    {
        _metricId = metricId;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var rect = GetRect();
        var plotRect = new Rect2(78.0f, 18.0f, rect.Size.X - 112.0f, rect.Size.Y - 62.0f);
        DrawRect(plotRect, new Color(0.08f, 0.10f, 0.12f), filled: true);
        DrawRect(plotRect, new Color(0.21f, 0.25f, 0.29f), filled: false, width: 1.0f);

        var snapshots = _runHistory?.Snapshots;
        var hasData = snapshots is not null && snapshots.Count > 0;
        var range = MetricCatalog.GetDefaultRange(_metricId);
        var minValue = range.X;
        var maxValue = range.Y;

        var metricColor = MetricCatalog.GetColor(_metricId);
        var pointCount = hasData ? snapshots!.Count : 0;
        var previousPoint = Vector2.Zero;
        var xAxisMax = hasData ? Mathf.Max(100, snapshots![^1].GenerationIndex) : 100;
        DrawAxes(plotRect, minValue, maxValue, xAxisMax);

        for (var index = 0; index < pointCount; index++)
        {
            var snapshot = snapshots![index];
            var value = snapshot.GetMetricValue(_metricId);
            var x = pointCount == 1
                ? plotRect.Position.X + (plotRect.Size.X * 0.5f)
                : plotRect.Position.X + ((float)snapshot.GenerationIndex / xAxisMax * plotRect.Size.X);
            var normalizedY = Mathf.InverseLerp(minValue, maxValue, value);
            var y = plotRect.End.Y - (normalizedY * plotRect.Size.Y);
            var point = new Vector2(x, y);

            if (index > 0)
            {
                DrawLine(previousPoint, point, metricColor, width: 2.0f);
            }

            DrawCircle(point, 2.6f, metricColor);
            previousPoint = point;
        }

        DrawString(
            ThemeDB.FallbackFont,
            new Vector2(plotRect.Position.X, 18.0f),
            $"{MetricCatalog.GetLabel(_metricId)}",
            fontSize: 16,
            modulate: new Color(0.92f, 0.94f, 0.97f));
        DrawString(
            ThemeDB.FallbackFont,
            new Vector2(plotRect.End.X - 112.0f, rect.End.Y - 16.0f),
            "Generation",
            fontSize: 14,
            modulate: new Color(0.72f, 0.76f, 0.82f));

        if (!hasData)
        {
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(plotRect.Position.X + 16.0f, plotRect.Position.Y + 28.0f),
                "Waiting for generation data...",
                fontSize: 14,
                modulate: new Color(0.56f, 0.61f, 0.67f));
        }
    }

    private void DrawAxes(Rect2 plotRect, float minValue, float maxValue, int maxGenerationIndex)
    {
        var axisColor = new Color(0.45f, 0.50f, 0.56f);
        var gridColor = new Color(0.18f, 0.22f, 0.26f, 0.7f);
        const int yTickCount = 4;
        const int xTickCount = 4;

        DrawLine(
            new Vector2(plotRect.Position.X, plotRect.Position.Y),
            new Vector2(plotRect.Position.X, plotRect.End.Y),
            axisColor,
            width: 1.0f);
        DrawLine(
            new Vector2(plotRect.Position.X, plotRect.End.Y),
            plotRect.End,
            axisColor,
            width: 1.0f);

        for (var tick = 0; tick <= yTickCount; tick++)
        {
            var ratio = (float)tick / yTickCount;
            var y = plotRect.End.Y - (ratio * plotRect.Size.Y);
            DrawLine(
                new Vector2(plotRect.Position.X, y),
                new Vector2(plotRect.End.X, y),
                gridColor,
                width: 1.0f);
            DrawLine(
                new Vector2(plotRect.Position.X - 6.0f, y),
                new Vector2(plotRect.Position.X, y),
                axisColor,
                width: 1.0f);

            var tickValue = Mathf.Lerp(minValue, maxValue, ratio);
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(12.0f, y + 5.0f),
                $"{tickValue:0.##}",
                fontSize: 12,
                modulate: new Color(0.68f, 0.72f, 0.78f));
        }

        for (var tick = 0; tick <= xTickCount; tick++)
        {
            var ratio = (float)tick / xTickCount;
            var x = plotRect.Position.X + (ratio * plotRect.Size.X);
            DrawLine(
                new Vector2(x, plotRect.End.Y),
                new Vector2(x, plotRect.End.Y + 6.0f),
                axisColor,
                width: 1.0f);

            var generationValue = Mathf.RoundToInt(ratio * maxGenerationIndex);
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(x - 10.0f, plotRect.End.Y + 22.0f),
                generationValue.ToString(),
                fontSize: 12,
                modulate: new Color(0.68f, 0.72f, 0.78f));
        }
    }
}
