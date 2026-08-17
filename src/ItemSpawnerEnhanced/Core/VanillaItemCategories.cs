using System;
using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Core;

internal static class VanillaItemCategories
{
    private static readonly IReadOnlyDictionary<string, ItemFilterTag> Tags = BuildTags();

    public static ItemFilterTag Resolve(string displayName, string prefabName)
    {
        if (!Tags.TryGetValue(displayName, out ItemFilterTag tags))
            Tags.TryGetValue(prefabName, out tags);

        return tags;
    }

    private static IReadOnlyDictionary<string, ItemFilterTag> BuildTags()
    {
        var tags = new Dictionary<string, ItemFilterTag>(StringComparer.OrdinalIgnoreCase);
        Add(tags, ItemFilterTag.Food,
            "Airline Food", "Beehive", "Big Egg", "Big Lollipop", "Black Clusterberry",
            "Blue Berrynana", "Blue Shroomberry", "Brown Berrynana", "Bugle Shroom",
            "Button Shroom", "Chubby Shroom", "Cluster Shroom", "Coconut", "Cooked Bird",
            "Egg", "Energy Drink", "Fortified Milk", "Frog Legs", "Gold Prickleberry",
            "Granola Bar", "Green Crispberry", "Green Kingberry", "Green Shroomberry",
            "Coconut Half", "Half-Coconut", "Honeycomb", "Hot Dog", "Mandrake",
            "Marshmallow", "Medicinal Root", "Napberry", "Orange Winterberry",
            "Pink Berrynana", "Purple Kingberry", "Purple Shroomberry", "Red Clusterberry",
            "Red Crispberry", "Red Prickleberry", "Red Shroomberry", "Scorchberry",
            "Scorpion", "Scout Cookies", "Small Egg", "Sports Drink", "The Early Worm",
            "Tick", "Trail Mix", "Yellow Berrynana", "Yellow Clusterberry",
            "Yellow Crispberry", "Yellow Kingberry", "Yellow Shroomberry",
            "Yellow Winterberry");
        Add(tags, ItemFilterTag.Consumable,
            "Aloe Vera", "Antidote", "Anti-Zooka", "Balloon", "Balloon Bunch", "Bandages",
            "Blowgun", "Bugle of Friendship", "Candlestick", "Cure-All", "Cursed Skull",
            "Dynamite", "Faerie Lantern", "First Aid Kit", "Flare", "Heat Pack", "Lantern",
            "Pandora's Lunchbox", "Parachute", "Remedy Fungus", "Rescue Claw",
            "Ritual Dagger", "Scout Effigy", "Scoutmaster's Bugle", "Sunscreen",
            "The Book of Bones", "Torch", "Warp Compass", "Warp Fungus");
        Add(tags, ItemFilterTag.Equipment,
            "Ancient Idol", "Backpack", "Balloon", "Balloon Bunch", "Binoculars",
            "Bugle", "Bugle?", "Compass", "Fanny Pack", "Fannypack", "Glider",
            "Jetpack", "Parasol", "Parachute", "Pirate's Compass", "Rocketpack");
        Add(tags, ItemFilterTag.Deployable,
            "Anti-Rope Cannon", "Anti-Rope Spool", "Bounce Fungus", "Chain Launcher",
            "Checkpoint Flag", "Cloud Fungus", "Magic Bean", "Piton", "Portable Stove",
            "Rope Cannon", "Rope Spool", "Scout Cannon", "Shelf Fungus");
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
