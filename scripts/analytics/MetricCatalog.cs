using Godot;

namespace EvoPrototype.Analytics;

public static class MetricCatalog
{
    private static readonly MetricId[] AllMetrics =
    [
        MetricId.DeathCount,
        MetricId.SurvivalRate,
        MetricId.AverageMass,
        MetricId.AverageBaseThrottle
    ];

    public static int Count => AllMetrics.Length;

    public static MetricId GetByIndex(int index)
    {
        var wrappedIndex = ((index % AllMetrics.Length) + AllMetrics.Length) % AllMetrics.Length;
        return AllMetrics[wrappedIndex];
    }

    public static string GetLabel(MetricId metricId)
    {
        return metricId switch
        {
            MetricId.DeathCount => "Deaths / generation",
            MetricId.SurvivalRate => "Survival rate",
            MetricId.AverageMass => "Average mass",
            MetricId.AverageBaseThrottle => "Average base throttle",
            _ => metricId.ToString()
        };
    }

    public static Color GetColor(MetricId metricId)
    {
        return metricId switch
        {
            MetricId.DeathCount => new Color(0.96f, 0.42f, 0.42f),
            MetricId.SurvivalRate => new Color(0.45f, 0.86f, 0.55f),
            MetricId.AverageMass => new Color(0.54f, 0.74f, 0.98f),
            MetricId.AverageBaseThrottle => new Color(0.96f, 0.84f, 0.36f),
            _ => Colors.White
        };
    }

    public static Vector2 GetDefaultRange(MetricId metricId)
    {
        return metricId switch
        {
            MetricId.DeathCount => new Vector2(0.0f, 200.0f),
            MetricId.SurvivalRate => new Vector2(0.0f, 1.0f),
            MetricId.AverageMass => new Vector2(0.0f, 3.0f),
            MetricId.AverageBaseThrottle => new Vector2(0.0f, 1.0f),
            _ => new Vector2(0.0f, 1.0f)
        };
    }
}
