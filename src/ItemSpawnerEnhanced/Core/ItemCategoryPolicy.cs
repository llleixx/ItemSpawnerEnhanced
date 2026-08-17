namespace ItemSpawnerEnhanced.Core;

internal static class ItemCategoryPolicy
{
    public static ItemFilterTag ApplyConsumptionTags(
        ItemFilterTag tags,
        bool consumes,
        bool hasFoodEvidence)
    {
        if (hasFoodEvidence || tags.HasFlag(ItemFilterTag.Food))
            return (tags | ItemFilterTag.Food) & ~ItemFilterTag.Consumable;

        return consumes ? tags | ItemFilterTag.Consumable : tags;
    }

    public static ItemFilterTag NormalizeEquipmentTag(ItemFilterTag tags)
    {
        const ItemFilterTag incompatibleTags = ItemFilterTag.Food | ItemFilterTag.Deployable;
        return (tags & incompatibleTags) != 0 ? tags & ~ItemFilterTag.Equipment : tags;
    }

    public static ItemFilterTag NormalizeConsumableTag(ItemFilterTag tags)
    {
        return tags.HasFlag(ItemFilterTag.Deployable)
            ? tags & ~ItemFilterTag.Consumable
            : tags;
    }
}
