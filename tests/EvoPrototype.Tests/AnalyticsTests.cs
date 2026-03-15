using EvoPrototype.Analytics;
using EvoPrototype.Core;
using static TestAssertions;
using static TestFixtures;

public static class AnalyticsTests
{
    public static void Register(TestHarness harness)
    {
        harness.Run("Statistics collector captures deaths and averages", TestStatisticsCollectorCapturesDeathsAndAverages);
        harness.Run("Run history keeps snapshots in generation order", TestRunHistoryKeepsSnapshotsInGenerationOrder);
        harness.Run("Statistics collector handles empty populations", TestStatisticsCollectorHandlesEmptyPopulations);
        harness.Run("Metric catalog wraps metric indices", TestMetricCatalogWrapsMetricIndices);
        harness.Run("Metric catalog ranges stay fixed and include zero", TestMetricCatalogRangesStayFixedAndIncludeZero);
        harness.Run("Live metrics handle averages and empty worlds", TestLiveMetricsHandleAveragesAndEmptyWorlds);
    }

    private static void TestStatisticsCollectorCapturesDeathsAndAverages()
    {
        var firstAgent = CreateAnalyticsAgent(new Godot.Vector2(5.0f, 10.0f), 1.2f, 0.4f);
        var secondAgent = CreateAnalyticsAgent(new Godot.Vector2(10.0f, 10.0f), 1.8f, 0.8f);
        var thirdAgent = CreateAnalyticsAgent(new Godot.Vector2(15.0f, 10.0f), 0.9f, 0.6f);

        var population = new[] { firstAgent, secondAgent, thirdAgent };
        var survivors = new[] { firstAgent, thirdAgent };
        var snapshot = StatisticsCollector.Capture(7, population, survivors);

        AssertTrue(snapshot.GenerationIndex == 7, "Snapshot should preserve the current generation index.");
        AssertTrue(snapshot.DeathCount == 1, "One agent should be counted as dead.");
        AssertNearlyEqual(2.0f / 3.0f, snapshot.SurvivalRate, 0.001f, "Survival rate should match survivor ratio.");
        AssertNearlyEqual((1.2f + 1.8f + 0.9f) / 3.0f, snapshot.AverageMass, 0.001f, "Average mass should be computed on the full evaluated population.");
        AssertNearlyEqual((0.4f + 0.8f + 0.6f) / 3.0f, snapshot.AverageBaseThrottle, 0.001f, "Average base throttle should be computed on the full evaluated population.");
    }

    private static void TestRunHistoryKeepsSnapshotsInGenerationOrder()
    {
        var runHistory = new RunHistory();
        runHistory.Add(new GenerationStatistics(0, 200, 100, 100, 0.5f, 1.0f, 0.8f));
        runHistory.Add(new GenerationStatistics(1, 200, 120, 80, 0.6f, 1.1f, 0.85f));

        AssertTrue(runHistory.Snapshots.Count == 2, "Run history should store each appended snapshot.");
        AssertTrue(runHistory.Snapshots[0].GenerationIndex == 0, "Run history should preserve insertion order.");
        AssertTrue(runHistory.Snapshots[1].GenerationIndex == 1, "Later generations should stay at the end of the history.");
    }

    private static void TestStatisticsCollectorHandlesEmptyPopulations()
    {
        var snapshot = StatisticsCollector.Capture(3, Array.Empty<Agent>(), Array.Empty<Agent>());

        AssertTrue(snapshot.GenerationIndex == 3, "Empty-population snapshots should still preserve generation index.");
        AssertTrue(snapshot.PopulationSize == 0, "Population size should stay zero for an empty input.");
        AssertTrue(snapshot.DeathCount == 0, "Empty populations should not report phantom deaths.");
        AssertNearlyEqual(0.0f, snapshot.SurvivalRate, 0.0001f, "Empty populations should report a zero survival rate.");
        AssertNearlyEqual(0.0f, snapshot.AverageMass, 0.0001f, "Empty populations should report zero-valued averages.");
        AssertNearlyEqual(0.0f, snapshot.AverageBaseThrottle, 0.0001f, "Empty populations should report zero-valued averages.");
    }

    private static void TestMetricCatalogWrapsMetricIndices()
    {
        AssertTrue(MetricCatalog.GetByIndex(-1) == MetricId.AverageBaseThrottle, "Negative metric indices should wrap to the end of the catalog.");
        AssertTrue(MetricCatalog.GetByIndex(MetricCatalog.Count) == MetricId.DeathCount, "Indices at catalog length should wrap back to the first metric.");
    }

    private static void TestMetricCatalogRangesStayFixedAndIncludeZero()
    {
        var deathRange = MetricCatalog.GetDefaultRange(MetricId.DeathCount);
        var survivalRange = MetricCatalog.GetDefaultRange(MetricId.SurvivalRate);
        var massRange = MetricCatalog.GetDefaultRange(MetricId.AverageMass);

        AssertNearlyEqual(0.0f, deathRange.X, 0.0001f, "Death count range should start at zero.");
        AssertNearlyEqual(200.0f, deathRange.Y, 0.0001f, "Death count range should cover the full default population.");
        AssertNearlyEqual(0.0f, survivalRange.X, 0.0001f, "Survival range should start at zero.");
        AssertNearlyEqual(1.0f, survivalRange.Y, 0.0001f, "Survival range should stay normalized to one.");
        AssertNearlyEqual(0.0f, massRange.X, 0.0001f, "Mass range should start at zero.");
        AssertTrue(massRange.Y >= 3.0f, "Mass range should leave headroom for mutated agents.");
    }

    private static void TestLiveMetricsHandleAveragesAndEmptyWorlds()
    {
        var emptyMetrics = LiveMetrics.FromAgents(Array.Empty<Agent>());
        AssertTrue(emptyMetrics.AgentCount == 0, "Empty worlds should report zero agents.");
        AssertNearlyEqual(0.0f, emptyMetrics.AverageEnergy, 0.0001f, "Empty worlds should report zero average energy.");
        AssertNearlyEqual(0.0f, emptyMetrics.AverageSpeed, 0.0001f, "Empty worlds should report zero average speed.");

        var firstAgent = CreateTestAgent(new Godot.Vector2(0.0f, 0.0f));
        firstAgent.Energy = 50.0f;
        firstAgent.Velocity = new Godot.Vector2(3.0f, 4.0f);
        var secondAgent = CreateTestAgent(new Godot.Vector2(1.0f, 1.0f));
        secondAgent.Energy = 70.0f;
        secondAgent.Velocity = new Godot.Vector2(0.0f, 10.0f);

        var metrics = LiveMetrics.FromAgents(new[] { firstAgent, secondAgent });
        AssertTrue(metrics.AgentCount == 2, "Live metrics should keep the current population count.");
        AssertNearlyEqual(60.0f, metrics.AverageEnergy, 0.0001f, "Average energy should be computed across agents.");
        AssertNearlyEqual(7.5f, metrics.AverageSpeed, 0.0001f, "Average speed should use vector length.");
    }
}
