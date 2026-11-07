namespace ItemSpawnerEnhanced.Core;

internal static class ItemCategoryPolicy
{
    public static ItemCategory ApplyConsumptionTags(
        ItemCategory tags,
        bool consumes,
        bool hasFoodEvidence)
    {
        if (hasFoodEvidence || tags.HasFlag(ItemCategory.Food))
            return (tags | ItemCategory.Food) & ~ItemCategory.Consumable;

        return consumes ? tags | ItemCategory.Consumable : tags;
    }

    public static ItemCategory Normalize(ItemCategory tags)
    {
        if (tags.HasFlag(ItemCategory.Deployable))
            tags &= ~ItemCategory.Consumable;
        const ItemCategory incompatibleTags = ItemCategory.Food | ItemCategory.Deployable;
        if ((tags & incompatibleTags) != 0)
            tags &= ~ItemCategory.Equipment;
        return tags == ItemCategory.None ? ItemCategory.Other : tags;
    }
}
