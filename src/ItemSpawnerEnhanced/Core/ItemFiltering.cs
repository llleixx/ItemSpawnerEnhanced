using System;

namespace ItemSpawnerEnhanced.Core;

[Flags]
internal enum ItemFilterTag
{
    None = 0,
    Favorite = 1 << 0,
    Food = 1 << 1,
    Consumable = 1 << 2,
    Equipment = 1 << 3,
    Deployable = 1 << 4,
    Mystical = 1 << 5,
    Other = 1 << 6
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
        ItemFilterTag itemTags,
        bool isFavorite,
        ItemFilterTag selectedTags,
        TagMatchMode matchMode)
    {
        if (selectedTags == ItemFilterTag.None)
            return true;

        if (isFavorite)
            itemTags |= ItemFilterTag.Favorite;

        return matchMode == TagMatchMode.And
            ? (itemTags & selectedTags) == selectedTags
            : (itemTags & selectedTags) != 0;
    }
}
