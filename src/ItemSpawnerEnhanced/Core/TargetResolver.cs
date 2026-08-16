using System.Collections.Generic;
using System.Linq;

namespace ItemSpawnerEnhanced.Core;

internal readonly struct TargetCandidate
{
    public TargetCandidate(int actorId, bool isLocal, bool isSpectated, bool isValid)
    {
        ActorId = actorId;
        IsLocal = isLocal;
        IsSpectated = isSpectated;
        IsValid = isValid;
    }

    public int ActorId { get; }
    public bool IsLocal { get; }
    public bool IsSpectated { get; }
    public bool IsValid { get; }
}

internal static class TargetResolver
{
    public static int? Resolve(IReadOnlyList<TargetCandidate> candidates, int? manualActorId)
    {
        if (manualActorId.HasValue)
        {
            TargetCandidate manual = candidates.FirstOrDefault(candidate =>
                candidate.ActorId == manualActorId.Value && candidate.IsValid);
            if (manual.IsValid)
                return manual.ActorId;
        }

        TargetCandidate spectated = candidates.FirstOrDefault(candidate => candidate.IsSpectated && candidate.IsValid);
        if (spectated.IsValid)
            return spectated.ActorId;

        TargetCandidate local = candidates.FirstOrDefault(candidate => candidate.IsLocal && candidate.IsValid);
        return local.IsValid ? local.ActorId : null;
    }
}

