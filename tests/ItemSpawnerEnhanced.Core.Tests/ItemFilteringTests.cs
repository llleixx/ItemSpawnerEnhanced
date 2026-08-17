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

    [TestCase("Bugle of Friendship", "BugleFriendship", (int)ItemFilterTag.Consumable)]
    [TestCase("Scoutmaster's Bugle", "ScoutmasterBugle", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Candlestick", "Candlestick", (int)ItemFilterTag.Consumable)]
    [TestCase("Lantern", "Lantern", (int)ItemFilterTag.Consumable)]
    [TestCase("Faerie Lantern", "Lantern_Faerie", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Rescue Claw", "RescueHook", (int)ItemFilterTag.Consumable)]
    [TestCase("Rescue Claw", "RescueHook_Infinite", (int)ItemFilterTag.Consumable)]
    [TestCase("Torch", "Torch", (int)ItemFilterTag.Consumable)]
    [TestCase("Warp Compass", "Warp Compass", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Ancient Idol", "Ancient Idol", (int)(ItemFilterTag.Equipment | ItemFilterTag.Mystical))]
    [TestCase("The Book of Bones", "BookOfBones", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Ritual Dagger", "RitualDagger", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    [TestCase("Anti-Zooka", "Anti-Zooka", (int)(ItemFilterTag.Consumable | ItemFilterTag.Mystical))]
    public void ReviewedItems_HaveExpectedTags(
        string displayName,
        string prefabName,
        int expectedTagValue)
    {
        var expected = (ItemFilterTag)expectedTagValue;
        Assert.That(VanillaItemCategories.Resolve(displayName, prefabName), Is.EqualTo(expected));
    }
}
