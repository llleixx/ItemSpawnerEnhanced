using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class RefreshStateTests
{
    [Test]
    public void CatalogRequest_AlsoRequiresSearchIndex()
    {
        var state = new RefreshState();

        Assert.That(
            state.Pending,
            Is.EqualTo(RefreshRequirement.Catalog | RefreshRequirement.SearchIndex));
    }

    [Test]
    public void SuccessfulPhases_ClearOnlyCompletedRequirement()
    {
        var state = new RefreshState();

        state.Begin(RebuildPhase.Catalog);
        state.Complete(RebuildPhase.Catalog);
        Assert.That(state.Pending, Is.EqualTo(RefreshRequirement.SearchIndex));

        state.Begin(RebuildPhase.SearchIndex);
        state.Complete(RebuildPhase.SearchIndex);
        state.Finish();

        Assert.Multiple(() =>
        {
            Assert.That(state.Pending, Is.EqualTo(RefreshRequirement.None));
            Assert.That(state.IsRebuilding, Is.False);
        });
    }

    [Test]
    public void Abort_PreservesPendingRequirementForRetry()
    {
        var state = new RefreshState();
        state.Begin(RebuildPhase.Catalog);

        state.Abort();

        Assert.Multiple(() =>
        {
            Assert.That(state.IsRebuilding, Is.False);
            Assert.That(
                state.Pending,
                Is.EqualTo(RefreshRequirement.Catalog | RefreshRequirement.SearchIndex));
        });
    }
}
