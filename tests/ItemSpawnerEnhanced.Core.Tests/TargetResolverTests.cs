using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class TargetResolverTests
{
    private static readonly TargetCandidate[] Candidates =
    {
        new(1, isLocal: true, isSpectated: false, isValid: true),
        new(2, isLocal: false, isSpectated: true, isValid: true),
        new(3, isLocal: false, isSpectated: false, isValid: true)
    };

    [Test]
    public void SmartTarget_PrefersSpectatedCharacter() =>
        Assert.That(TargetResolver.Resolve(Candidates, null), Is.EqualTo(2));

    [Test]
    public void ManualTarget_OverridesSmartTarget() =>
        Assert.That(TargetResolver.Resolve(Candidates, 3), Is.EqualTo(3));

    [Test]
    public void MissingManualTarget_FallsBackToSmartTarget() =>
        Assert.That(TargetResolver.Resolve(Candidates, 99), Is.EqualTo(2));

    [Test]
    public void SmartTarget_FallsBackToLocalCharacter() =>
        Assert.That(TargetResolver.Resolve(new[] { Candidates[0], Candidates[2] }, null), Is.EqualTo(1));
}
