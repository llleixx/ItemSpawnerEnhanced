using System;
using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;

namespace ItemSpawnerEnhanced;

internal static class ItemCategoryResolver
{
    private static readonly IReadOnlyDictionary<string, ItemFilterTag> VanillaTags = BuildVanillaTags();

    public static ItemFilterTag Resolve(Item item, string rawName)
    {
        ItemFilterTag tags = ItemFilterTag.None;
        if (!VanillaTags.TryGetValue(rawName, out tags))
            VanillaTags.TryGetValue(item.name, out tags);

        if (item.itemTags.HasFlag(Item.ItemTags.Mystical))
            tags |= ItemFilterTag.Mystical;

        bool consumes = item.GetComponentsInChildren<Action_Consume>(includeInactive: true).Length > 0;
        bool modifiesHunger = item.GetComponentsInChildren<Action_ModifyStatus>(includeInactive: true)
            .Any(action => action.statusType == CharacterAfflictions.STATUSTYPE.Hunger);
        bool hasFoodTag = item.itemTags.HasFlag(Item.ItemTags.PackagedFood) ||
                          item.itemTags.HasFlag(Item.ItemTags.Berry) ||
                          item.itemTags.HasFlag(Item.ItemTags.Mushroom);

        if (hasFoodTag || (consumes && modifiesHunger))
            tags |= ItemFilterTag.Food;
        else if (consumes)
            tags |= ItemFilterTag.Consumable;

        if (tags == ItemFilterTag.None)
            tags = ItemFilterTag.Other;

        return tags;
    }

    private static IReadOnlyDictionary<string, ItemFilterTag> BuildVanillaTags()
    {
        var tags = new Dictionary<string, ItemFilterTag>(StringComparer.OrdinalIgnoreCase);
        Add(tags, ItemFilterTag.Food,
            "Airline Food", "Big Lollipop", "Black Clusterberry", "Blue Berrynana",
            "Blue Shroomberry", "Brown Berrynana", "Bugle Shroom", "Button Shroom",
            "Chubby Shroom", "Cluster Shroom", "Cooked Bird", "Egg", "Energy Drink",
            "Fortified Milk", "Frog Legs", "Gold Prickleberry", "Granola Bar",
            "Green Crispberry", "Green Kingberry", "Green Shroomberry", "Half-Coconut",
            "Honeycomb", "Hot Dog", "Mandrake", "Marshmallow", "Medicinal Root",
            "Napberry", "Orange Winterberry", "Pink Berrynana", "Purple Kingberry",
            "Purple Shroomberry", "Red Clusterberry", "Red Crispberry", "Red Prickleberry",
            "Red Shroomberry", "Scorchberry", "Scorpion", "Scout Cookies", "Sports Drink",
            "The Early Worm", "Tick", "Trail Mix", "Yellow Berrynana", "Yellow Clusterberry",
            "Yellow Crispberry", "Yellow Kingberry", "Yellow Shroomberry", "Yellow Winterberry");
        Add(tags, ItemFilterTag.Consumable,
            "Aloe Vera", "Antidote", "Balloon", "Balloon Bunch", "Bandages", "Blowgun",
            "Cure-All", "Cursed Skull", "Dynamite", "First Aid Kit", "Flare", "Heat Pack",
            "Pandora's Lunchbox", "Remedy Fungus", "Rescue Claw", "Scout Effigy",
            "Scoutmaster's Bugle", "Sunscreen", "The Book of Bones", "Warp Fungus");
        Add(tags, ItemFilterTag.Equipment,
            "Backpack", "Binoculars", "Bugle", "Bugle of Friendship", "Bugle?", "Candlestick",
            "Compass", "Faerie Lantern", "Fanny Pack", "Guidebook", "Jetpack", "Lantern",
            "Parasol", "Pirate's Compass", "Rescue Claw", "Rocketpack", "Scoutmaster's Bugle",
            "Torch", "Warp Compass");
        Add(tags, ItemFilterTag.Deployable,
            "Anti-Rope Cannon", "Anti-Rope Spool", "Anti-Zooka", "Bounce Fungus",
            "Chain Launcher", "Checkpoint Flag", "Cloud Fungus", "Magic Bean", "Piton",
            "Portable Stove", "Rope Cannon", "Rope Spool", "Scout Cannon", "Shelf Fungus");
        Add(tags, ItemFilterTag.Mystical,
            "Ancient Idol", "Anti-Rope Cannon", "Anti-Rope Spool", "Anti-Zooka", "Cure-All",
            "Cursed Skull", "Faerie Lantern", "Pandora's Lunchbox", "Ritual Dagger",
            "Scout Effigy", "Scout's Ambition", "Scout's Generosity", "Scout's Honor",
            "Scout's Initiative", "Scout's Tenacity", "Scoutmaster's Bugle", "Strange Gem",
            "The Book of Bones", "Warp Compass");
        return tags;
    }

    private static void Add(
        IDictionary<string, ItemFilterTag> destination,
        ItemFilterTag tag,
        params string[] itemNames)
    {
        foreach (string itemName in itemNames)
        {
            destination.TryGetValue(itemName, out ItemFilterTag existing);
            destination[itemName] = existing | tag;
        }
    }
}
