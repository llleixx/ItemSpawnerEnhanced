using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ItemFilteringTests
{
    [Test]
    public void SingleTagSelection_ReplacesCurrentSelection()
    {
        ItemFilterTag selected = ItemFilterTag.Food | ItemFilterTag.Mystical;

        ItemFilterTag result = ItemTagSelection.Update(
            selected,
            ItemFilterTag.Equipment,
            selected: true,
            singleTagSelection: true);

        Assert.That(result, Is.EqualTo(ItemFilterTag.Equipment));
    }

    [Test]
    public void MultiTagSelection_AddsToCurrentSelection()
    {
        ItemFilterTag result = ItemTagSelection.Update(
            ItemFilterTag.Food,
            ItemFilterTag.Mystical,
            selected: true,
            singleTagSelection: false);

        Assert.That(result, Is.EqualTo(ItemFilterTag.Food | ItemFilterTag.Mystical));
    }

    [Test]
    public void Deselect_RemovesTagInEitherSelectionMode()
    {
        ItemFilterTag result = ItemTagSelection.Update(
            ItemFilterTag.Food | ItemFilterTag.Mystical,
            ItemFilterTag.Food,
            selected: false,
            singleTagSelection: true);

        Assert.That(result, Is.EqualTo(ItemFilterTag.Mystical));
    }

    [Test]
    public void NormalizeSingle_KeepsOneSelectedTag()
    {
        ItemFilterTag result = ItemTagSelection.NormalizeSingle(
            ItemFilterTag.Food | ItemFilterTag.Equipment | ItemFilterTag.Mystical);

        Assert.That(result, Is.EqualTo(ItemFilterTag.Food));
    }

    [Test]
    public void NoSelectedTags_MatchesEveryItem()
    {
        Assert.That(ItemFilterMatcher.Matches(
            ItemCategory.Other,
            ItemOrigin.Vanilla,
            isFavorite: false,
            ItemFilterTag.None,
            TagMatchMode.And), Is.True);
    }

    [Test]
    public void And_RequiresEverySelectedTag()
    {
        ItemFilterTag selected = ItemFilterTag.Favorite | ItemFilterTag.Equipment | ItemFilterTag.Mystical;
        ItemCategory item = ItemCategory.Equipment | ItemCategory.Mystical;

        Assert.Multiple(() =>
        {
            Assert.That(ItemFilterMatcher.Matches(item, ItemOrigin.Vanilla, true, selected, TagMatchMode.And), Is.True);
            Assert.That(ItemFilterMatcher.Matches(item, ItemOrigin.Vanilla, false, selected, TagMatchMode.And), Is.False);
        });
    }

    [Test]
    public void Or_RequiresAnySelectedTag()
    {
        ItemFilterTag selected = ItemFilterTag.Food | ItemFilterTag.Equipment;

        Assert.Multiple(() =>
        {
            Assert.That(ItemFilterMatcher.Matches(
                ItemCategory.Food,
                ItemOrigin.Vanilla,
                false,
                selected,
                TagMatchMode.Or), Is.True);
            Assert.That(ItemFilterMatcher.Matches(
                ItemCategory.Deployable,
                ItemOrigin.Vanilla,
                false,
                selected,
                TagMatchMode.Or), Is.False);
        });
    }

    [Test]
    public void Favorite_IsAppliedAsADynamicTag()
    {
        Assert.That(ItemFilterMatcher.Matches(
            ItemCategory.Other,
            ItemOrigin.Vanilla,
            true,
            ItemFilterTag.Favorite,
            TagMatchMode.And), Is.True);
    }

    [Test]
    public void Food_IsNeverAlsoTaggedAsConsumable()
    {
        ItemCategory tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemCategory.Food | ItemCategory.Mystical,
            consumes: true,
            hasFoodEvidence: false);

        Assert.That(tags, Is.EqualTo(ItemCategory.Food | ItemCategory.Mystical));
    }

    [Test]
    public void FoodEvidence_ReplacesAnExistingConsumableTag()
    {
        ItemCategory tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemCategory.Consumable,
            consumes: true,
            hasFoodEvidence: true);

        Assert.That(tags, Is.EqualTo(ItemCategory.Food));
    }

    [TestCase((int)ItemCategory.Food)]
    [TestCase((int)ItemCategory.Deployable)]
    [TestCase((int)(ItemCategory.Food | ItemCategory.Deployable | ItemCategory.Mystical))]
    public void FoodAndDeployableItems_AreNeverAlsoTaggedAsEquipment(int itemTagValue)
    {
        var itemTags = (ItemCategory)itemTagValue;
        ItemCategory tags = ItemCategoryPolicy.Normalize(
            itemTags | ItemCategory.Equipment);

        Assert.That(tags, Is.EqualTo(itemTags));
    }

    [Test]
    public void Deployable_IsNeverAlsoTaggedAsConsumable()
    {
        ItemCategory tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemCategory.Deployable | ItemCategory.Mystical,
            consumes: true,
            hasFoodEvidence: false);

        tags = ItemCategoryPolicy.Normalize(tags);

        Assert.That(tags, Is.EqualTo(ItemCategory.Deployable | ItemCategory.Mystical));
    }

    [TestCase("Bugle_Scoutmaster Variant", (int)(ItemCategory.Consumable | ItemCategory.Mystical))]
    [TestCase("AncientIdol", (int)(ItemCategory.Equipment | ItemCategory.Mystical))]
    [TestCase("RitualDagger", (int)(ItemCategory.Consumable | ItemCategory.Mystical))]
    public void ReviewedPrefabs_HaveExpectedTags(string prefabName, int expectedTagValue)
    {
        var expected = (ItemCategory)expectedTagValue;
        Assert.That(VanillaItemCategories.Resolve(prefabName), Is.EqualTo(expected));
    }

    [Test]
    public void ModdedAndFavoriteUseExistingAndOrRules()
    {
        ItemFilterTag selected = ItemFilterTag.Modded | ItemFilterTag.Favorite;
        Assert.Multiple(() =>
        {
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Modded, true,
                selected, TagMatchMode.And), Is.True);
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Vanilla, true,
                selected, TagMatchMode.And), Is.False);
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Modded, false,
                selected, TagMatchMode.Or), Is.True);
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Unknown, true,
                selected, TagMatchMode.Or), Is.True);
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Unknown, false,
                ItemFilterTag.Modded, TagMatchMode.And), Is.False);
        });
    }

    [Test]
    public void OriginPreservesCategoryAndOtherFallback()
    {
        ItemCategory other = ItemCategoryPolicy.Normalize(ItemCategory.None);
        Assert.Multiple(() =>
        {
            Assert.That(other, Is.EqualTo(ItemCategory.Other));
            Assert.That(ItemFilterMatcher.Matches(other, ItemOrigin.Modded, false,
                ItemFilterTag.Other | ItemFilterTag.Modded, TagMatchMode.And), Is.True);
            Assert.That(ItemFilterMatcher.Matches(ItemCategory.Food, ItemOrigin.Modded, false,
                ItemFilterTag.Food | ItemFilterTag.Modded, TagMatchMode.And), Is.True);
            Assert.That(ItemFilterMatcher.Matches(other, ItemOrigin.Unknown, false,
                ItemFilterTag.Other, TagMatchMode.And), Is.True);
        });
    }

    [Test]
    public void ReviewedPrefabMatching_IsExact()
    {
        Assert.That(VanillaItemCategories.Resolve("bookofbones"), Is.EqualTo(ItemCategory.None));
    }
}
