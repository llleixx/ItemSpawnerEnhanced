using System;

namespace ItemSpawnerEnhanced.Core;

[Flags]
internal enum ItemFilterTag
{
    None = 0,
    Favorite = 1 << 0,
    Food = (int)ItemCategory.Food,
    Consumable = (int)ItemCategory.Consumable,
    Equipment = (int)ItemCategory.Equipment,
    Deployable = (int)ItemCategory.Deployable,
    Mystical = (int)ItemCategory.Mystical,
    Other = (int)ItemCategory.Other,
    Modded = 1 << 7
}

internal enum TagMatchMode
{
    And,
    Or
}

internal static class ItemTagSelection
{
    public static ItemFilterTag Update(
        ItemFilterTag selectedTags,
        ItemFilterTag changedTag,
        bool selected,
        bool singleTagSelection)
    {
        if (!selected)
            return selectedTags & ~changedTag;

        return singleTagSelection ? changedTag : selectedTags | changedTag;
    }

    public static ItemFilterTag NormalizeSingle(ItemFilterTag selectedTags)
    {
        int value = (int)selectedTags;
        return value == 0 ? ItemFilterTag.None : (ItemFilterTag)(value & -value);
    }
}

internal static class ItemFilterMatcher
{
    public static bool Matches(
        ItemCategory categories,
        ItemOrigin origin,
        bool isFavorite,
        ItemFilterTag selectedTags,
        TagMatchMode matchMode)
    {
        if (selectedTags == ItemFilterTag.None)
            return true;

        ItemFilterTag itemTags = (ItemFilterTag)categories;
        if (origin == ItemOrigin.Modded)
            itemTags |= ItemFilterTag.Modded;
        if (isFavorite)
            itemTags |= ItemFilterTag.Favorite;

        return matchMode == TagMatchMode.And
            ? (itemTags & selectedTags) == selectedTags
            : (itemTags & selectedTags) != 0;
    }
}
