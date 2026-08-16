using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Core;

internal readonly struct TargetCandidate
{
    public TargetCandidate(
        int actorId,
        bool isLocal,
        bool isSpectated,
        bool isSelectable,
        bool canReceiveItem)
    {
        ActorId = actorId;
        IsLocal = isLocal;
        IsSpectated = isSpectated;
        IsSelectable = isSelectable;
        CanReceiveItem = canReceiveItem;
    }

    public int ActorId { get; }
    public bool IsLocal { get; }
    public bool IsSpectated { get; }
    public bool IsSelectable { get; }
    public bool CanReceiveItem { get; }
}

internal static class TargetResolver
{
    public static int? ResolveIndex(IReadOnlyList<TargetCandidate> candidates, int? manualActorId)
    {
        if (manualActorId.HasValue)
        {
            for (int index = 0; index < candidates.Count; index++)
            {
                TargetCandidate candidate = candidates[index];
                if (candidate.ActorId == manualActorId.Value && candidate.IsSelectable)
                    return index;
            }
        }

        for (int index = 0; index < candidates.Count; index++)
        {
            TargetCandidate candidate = candidates[index];
            if (candidate.IsSpectated && candidate.CanReceiveItem)
                return index;
        }

        for (int index = 0; index < candidates.Count; index++)
        {
            TargetCandidate candidate = candidates[index];
            if (candidate.IsLocal && candidate.CanReceiveItem)
                return index;
        }

        return null;
    }
}
