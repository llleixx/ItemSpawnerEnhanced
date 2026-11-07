using System.Linq;
using ItemSpawnerEnhanced.Core;

namespace ItemSpawnerEnhanced;

internal static class ItemCategoryResolver
{
    public static ItemCategory Resolve(Item item)
    {
        ItemCategory tags = VanillaItemCategories.Resolve(item.name);

        if (item.itemTags.HasFlag(Item.ItemTags.Mystical))
            tags |= ItemCategory.Mystical;

        bool consumes = item.GetComponentsInChildren<Action_Consume>(includeInactive: true).Length > 0;
        bool modifiesHunger = item.GetComponentsInChildren<Action_ModifyStatus>(includeInactive: true)
            .Any(action => action.statusType == CharacterAfflictions.STATUSTYPE.Hunger);
        bool restoresHunger =
            item.GetComponentsInChildren<Action_RestoreHunger>(includeInactive: true).Length > 0;
        bool hasFoodTag = item.itemTags.HasFlag(Item.ItemTags.PackagedFood) ||
                          item.itemTags.HasFlag(Item.ItemTags.Berry) ||
                          item.itemTags.HasFlag(Item.ItemTags.Mushroom);

        tags = ItemCategoryPolicy.ApplyConsumptionTags(
            tags,
            consumes,
            hasFoodTag || restoresHunger || (consumes && modifiesHunger));
        return ItemCategoryPolicy.Normalize(tags);
    }
}
