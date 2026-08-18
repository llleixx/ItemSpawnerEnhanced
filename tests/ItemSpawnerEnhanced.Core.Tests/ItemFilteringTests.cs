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

    [Test]
    public void Food_IsNeverAlsoTaggedAsConsumable()
    {
        ItemFilterTag tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemFilterTag.Food | ItemFilterTag.Mystical,
            consumes: true,
            hasFoodEvidence: false);

        Assert.That(tags, Is.EqualTo(ItemFilterTag.Food | ItemFilterTag.Mystical));
    }

    [Test]
    public void FoodEvidence_ReplacesAnExistingConsumableTag()
    {
        ItemFilterTag tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemFilterTag.Consumable,
            consumes: true,
            hasFoodEvidence: true);

        Assert.That(tags, Is.EqualTo(ItemFilterTag.Food));
    }

    [TestCase((int)ItemFilterTag.Food)]
    [TestCase((int)ItemFilterTag.Deployable)]
    [TestCase((int)(ItemFilterTag.Food | ItemFilterTag.Deployable | ItemFilterTag.Mystical))]
    public void FoodAndDeployableItems_AreNeverAlsoTaggedAsEquipment(int itemTagValue)
    {
        var itemTags = (ItemFilterTag)itemTagValue;
        ItemFilterTag tags = ItemCategoryPolicy.NormalizeEquipmentTag(
            itemTags | ItemFilterTag.Equipment);

        Assert.That(tags, Is.EqualTo(itemTags));
    }

    [Test]
    public void Deployable_IsNeverAlsoTaggedAsConsumable()
    {
        ItemFilterTag tags = ItemCategoryPolicy.ApplyConsumptionTags(
            ItemFilterTag.Deployable | ItemFilterTag.Mystical,
            consumes: true,
            hasFoodEvidence: false);

        tags = ItemCategoryPolicy.NormalizeConsumableTag(tags);

        Assert.That(tags, Is.EqualTo(ItemFilterTag.Deployable | ItemFilterTag.Mystical));
    }

    [TestCase("Bugle_Magic", (int)ItemFilterTag.Consumable)]
    [TestCase("Bugle_Scoutmaster Variant", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Candle", (int)ItemFilterTag.Consumable)]
    [TestCase("Lantern", (int)ItemFilterTag.Consumable)]
    [TestCase("Lantern_Faerie", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("RescueHook", (int)ItemFilterTag.Consumable)]
    [TestCase("RescueHook_Infinite", (int)ItemFilterTag.Consumable)]
    [TestCase("Torch", (int)ItemFilterTag.Consumable)]
    [TestCase("Warp Compass", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("AncientIdol", (int)(ItemFilterTag.Equipment | ItemFilterTag.Mystical))]
    [TestCase("BookOfBones", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("RitualDagger", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("AntiZooka", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    public void ReviewedPrefabs_HaveExpectedTags(string prefabName, int expectedTagValue)
    {
        var expected = (ItemFilterTag)expectedTagValue;
        Assert.That(VanillaItemCategories.Resolve(prefabName), Is.EqualTo(expected));
    }

    [Test]
    public void ReviewedPrefabMatching_IsExact()
    {
        Assert.That(VanillaItemCategories.Resolve("bookofbones"), Is.EqualTo(ItemFilterTag.None));
    }
}
