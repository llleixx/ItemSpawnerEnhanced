using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ItemFilteringTests
{
    [Test]
    public void NoSelectedTags_MatchesEveryItem()
    {
        Assert.That(ItemFilterMatcher.Matches(
            ItemFilterTag.Other,
            isFavorite: false,
            ItemFilterTag.None,
            TagMatchMode.And), Is.True);
    }

    [Test]
    public void And_RequiresEverySelectedTag()
    {
        ItemFilterTag selected = ItemFilterTag.Favorite | ItemFilterTag.Equipment | ItemFilterTag.Mystical;
        ItemFilterTag item = ItemFilterTag.Equipment | ItemFilterTag.Mystical;

        Assert.Multiple(() =>
        {
            Assert.That(ItemFilterMatcher.Matches(item, true, selected, TagMatchMode.And), Is.True);
            Assert.That(ItemFilterMatcher.Matches(item, false, selected, TagMatchMode.And), Is.False);
        });
    }

    [Test]
    public void Or_RequiresAnySelectedTag()
    {
        ItemFilterTag selected = ItemFilterTag.Food | ItemFilterTag.Equipment;

        Assert.Multiple(() =>
        {
            Assert.That(ItemFilterMatcher.Matches(
                ItemFilterTag.Food,
                false,
                selected,
                TagMatchMode.Or), Is.True);
            Assert.That(ItemFilterMatcher.Matches(
                ItemFilterTag.Deployable,
                false,
                selected,
                TagMatchMode.Or), Is.False);
        });
    }

    [Test]
    public void Favorite_IsAppliedAsADynamicTag()
    {
        Assert.That(ItemFilterMatcher.Matches(
            ItemFilterTag.Other,
            true,
            ItemFilterTag.Favorite,
            TagMatchMode.And), Is.True);
    }
}
