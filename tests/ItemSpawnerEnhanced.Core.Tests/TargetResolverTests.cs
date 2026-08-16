using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class TargetResolverTests
{
    private static readonly TargetCandidate[] Candidates =
    {
        new(1, isLocal: true, isSpectated: false, isSelectable: true, canReceiveItem: true),
        new(2, isLocal: false, isSpectated: true, isSelectable: true, canReceiveItem: true),
        new(3, isLocal: false, isSpectated: false, isSelectable: true, canReceiveItem: true)
    };

    [Test]
    public void SmartTarget_PrefersSpectatedCharacter() =>
        Assert.That(TargetResolver.ResolveIndex(Candidates, null), Is.EqualTo(1));

    [Test]
    public void ManualTarget_OverridesSmartTarget() =>
        Assert.That(TargetResolver.ResolveIndex(Candidates, 3), Is.EqualTo(2));

    [Test]
    public void MissingManualTarget_FallsBackToSmartTarget() =>
        Assert.That(TargetResolver.ResolveIndex(Candidates, 99), Is.EqualTo(1));

    [Test]
    public void SmartTarget_FallsBackToLocalCharacter() =>
        Assert.That(TargetResolver.ResolveIndex(new[] { Candidates[0], Candidates[2] }, null), Is.EqualTo(0));

    [Test]
    public void SmartTarget_AllowsAirportLobbyCharacterWithoutStableActorId()
    {
        var spectatedLobbyCharacter = new TargetCandidate(
            actorId: -1,
            isLocal: false,
            isSpectated: true,
            isSelectable: false,
            canReceiveItem: true);

        Assert.That(
            TargetResolver.ResolveIndex(new[] { Candidates[0], spectatedLobbyCharacter }, null),
            Is.EqualTo(1));
    }

    [Test]
    public void ManualTarget_AllowsSelectableAirportLobbyCharacter()
    {
        var remoteLobbyCharacter = new TargetCandidate(
            actorId: 4,
            isLocal: false,
            isSpectated: false,
            isSelectable: true,
            canReceiveItem: true);

        Assert.That(
            TargetResolver.ResolveIndex(new[] { Candidates[0], remoteLobbyCharacter }, 4),
            Is.EqualTo(1));
    }
}
