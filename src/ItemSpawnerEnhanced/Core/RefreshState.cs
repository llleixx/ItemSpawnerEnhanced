using System;

namespace ItemSpawnerEnhanced.Core;

[Flags]
internal enum RefreshRequirement
{
    None = 0,
    SearchIndex = 1,
    Catalog = 2
}

internal enum RebuildPhase
{
    Idle,
    Catalog,
    SearchIndex
}

internal sealed class RefreshState
{
    public RefreshState() => Request(RefreshRequirement.Catalog);

    public RefreshRequirement Pending { get; private set; }
    public RebuildPhase Phase { get; private set; }
    public bool IsRebuilding => Phase != RebuildPhase.Idle;

    public void Request(RefreshRequirement requirement)
    {
        Pending |= requirement;
        if ((requirement & RefreshRequirement.Catalog) != 0)
            Pending |= RefreshRequirement.SearchIndex;
    }

    public void Begin(RebuildPhase phase)
    {
        if (phase == RebuildPhase.Idle)
            throw new ArgumentOutOfRangeException(nameof(phase));
        Phase = phase;
    }

    public void Complete(RebuildPhase phase)
    {
        if (Phase != phase)
            throw new InvalidOperationException($"Cannot complete {phase} while rebuilding {Phase}.");

        Pending &= phase switch
        {
            RebuildPhase.Catalog => ~RefreshRequirement.Catalog,
            RebuildPhase.SearchIndex => ~RefreshRequirement.SearchIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(phase))
        };
    }

    public void Finish() => Phase = RebuildPhase.Idle;

    public void Abort() => Phase = RebuildPhase.Idle;
}
