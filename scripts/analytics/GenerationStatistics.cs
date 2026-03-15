namespace EvoPrototype.Analytics;

/// <summary>
/// Captures the key outcomes of one evaluated generation for later charting.
/// </summary>
public readonly record struct GenerationStatistics(
    int GenerationIndex,
    int PopulationSize,
    int SurvivorCount,
    int DeathCount,
    float SurvivalRate,
    float AverageMass,
    float AverageBaseThrottle)
{
    public float GetMetricValue(MetricId metricId)
    {
        return metricId switch
        {
            MetricId.DeathCount => DeathCount,
            MetricId.SurvivalRate => SurvivalRate,
            MetricId.AverageMass => AverageMass,
            MetricId.AverageBaseThrottle => AverageBaseThrottle,
            _ => 0.0f
        };
    }
}
