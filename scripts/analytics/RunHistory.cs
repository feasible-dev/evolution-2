using System.Collections.Generic;

namespace EvoPrototype.Analytics;

/// <summary>
/// Stores the per-generation history of a single sandbox run.
/// </summary>
public sealed class RunHistory
{
    private readonly List<GenerationStatistics> _snapshots = new();

    public IReadOnlyList<GenerationStatistics> Snapshots => _snapshots;

    public void Clear()
    {
        _snapshots.Clear();
    }

    public void Add(GenerationStatistics snapshot)
    {
        _snapshots.Add(snapshot);
    }
}
