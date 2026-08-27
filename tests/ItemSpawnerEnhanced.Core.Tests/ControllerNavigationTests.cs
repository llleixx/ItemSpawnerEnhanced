using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ControllerNavigationTests
{
    [TestCase(0, (int)ControllerNavigationDirection.Left, 0)]
    [TestCase(0, (int)ControllerNavigationDirection.Right, 1)]
    [TestCase(5, (int)ControllerNavigationDirection.Right, 5)]
    [TestCase(0, (int)ControllerNavigationDirection.Down, 6)]
    [TestCase(5, (int)ControllerNavigationDirection.Down, 7)]
    [TestCase(6, (int)ControllerNavigationDirection.Right, 7)]
    [TestCase(7, (int)ControllerNavigationDirection.Down, 7)]
    [TestCase(7, (int)ControllerNavigationDirection.Up, 1)]
    public void MoveInGrid_HandlesPartialLastRow(
        int current,
        int direction,
        int expected) =>
        Assert.That(
            ControllerNavigation.MoveInGrid(
                current,
                8,
                6,
                (ControllerNavigationDirection)direction),
            Is.EqualTo(expected));

    [Test]
    public void MoveInGrid_DoesNotMoveBelowCompleteLastRow() =>
        Assert.That(
            ControllerNavigation.MoveInGrid(10, 12, 6, ControllerNavigationDirection.Down),
            Is.EqualTo(10));

    [Test]
    public void FindNearest_UsesHorizontalDistanceAndStableFirstTie()
    {
        float[] positions = { 10, 20, 30 };
        Assert.Multiple(() =>
        {
            Assert.That(ControllerNavigation.FindNearest(24, positions), Is.EqualTo(1));
            Assert.That(ControllerNavigation.FindNearest(25, positions), Is.EqualTo(1));
            Assert.That(ControllerNavigation.FindNearest(25, System.Array.Empty<float>()), Is.EqualTo(-1));
        });
    }

    [TestCase(-3, 5, 0)]
    [TestCase(2, 5, 2)]
    [TestCase(9, 5, 4)]
    [TestCase(0, 0, -1)]
    public void ResolveFallbackIndex_ClampsToVisibleItems(int previous, int count, int expected) =>
        Assert.That(ControllerNavigation.ResolveFallbackIndex(previous, count), Is.EqualTo(expected));

    [Test]
    public void MoveRegion_WrapsAcrossAllThreeRegions()
    {
        bool[] available = { true, true, true };
        Assert.Multiple(() =>
        {
            Assert.That(ControllerNavigation.MoveRegion(0, 1, available), Is.EqualTo(1));
            Assert.That(ControllerNavigation.MoveRegion(0, -1, available), Is.EqualTo(2));
            Assert.That(ControllerNavigation.MoveRegion(2, 1, available), Is.EqualTo(0));
        });
    }

    [Test]
    public void MoveRegion_SkipsUnavailableItemRegion()
    {
        bool[] available = { true, true, false };
        Assert.Multiple(() =>
        {
            Assert.That(ControllerNavigation.MoveRegion(1, 1, available), Is.EqualTo(0));
            Assert.That(ControllerNavigation.MoveRegion(0, -1, available), Is.EqualTo(1));
        });
    }
}
