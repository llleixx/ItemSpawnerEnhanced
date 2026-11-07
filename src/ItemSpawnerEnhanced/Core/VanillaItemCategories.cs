using System;
using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Core;

internal static class VanillaItemCategories
{
    private static readonly IReadOnlyDictionary<string, ItemCategory> Categories = BuildCategories();

    public static IReadOnlyDictionary<string, ItemCategory> ReviewedCategories => Categories;

    public static ItemCategory Resolve(string prefabName) =>
        Categories.TryGetValue(prefabName, out ItemCategory tags) ? tags : ItemCategory.None;

    private static IReadOnlyDictionary<string, ItemCategory> BuildCategories()
    {
        var tags = new Dictionary<string, ItemCategory>(StringComparer.Ordinal);
        Add(tags, ItemCategory.Food,
            "Airplane Food", "Beehive", "NestEgg", "Lollipop", "Lollipop_Prop",
            "Clusterberry Black", "Berrynana Blue", "Shroomberry_Blue", "Berrynana Brown",
            "Mushroom Lace", "Mushroom Lace Poison", "Mushroom Normie Poison",
            "Mushroom Normie", "Mushroom Chubby", "Mushroom Cluster Poison",
            "Mushroom Cluster", "Item_Coconut", "EggTurkey", "EggRaven", "Egg",
            "Energy Drink", "FortifiedMilk", "FrogLegs", "Prickleberry_Gold", "Granola Bar",
            "Apple Berry Green", "C_Pawn_f Variant", "C_Bishop_m Variant", "C_Rook_m",
            "Kingberry Green", "C_Pawn_m", "C_Pawn_f", "C_Bishop_f", "C_Knight",
            "C_Bishop_m", "C_King", "C_Queen", "C_Rook_f", "C_Rook_f Variant",
            "Shroomberry_Green", "Item_Coconut_half", "Item_Honeycomb",
            "Glizzy_CattailVariant", "Glizzy", "Mandrake", "Marshmallow",
            "Mandrake_Hidden", "MedicinalRoot", "Napberry", "Winterberry Orange",
            "Berrynana Pink", "Kingberry Purple", "Shroomberry_Purple", "Clusterberry Red",
            "Apple Berry Red", "Prickleberry_Red", "Shroomberry_Red", "Pepper Berry",
            "Scorpion", "ScoutCookies", "ScoutCookies_Vanilla", "NestEgg_Raven",
            "Sports Drink", "EarlyWorm", "Bugfix", "TrailMix", "Berrynana Yellow",
            "Clusterberry Yellow", "Apple Berry Yellow", "Kingberry Yellow",
            "Shroomberry_Yellow", "Winterberry Yellow");
        Add(tags, ItemCategory.Consumable,
            "AloeVera", "Antidote", "AntiZooka", "Balloon", "BalloonBunch", "Bandages",
            "HealingDart Variant", "Bugle_Magic", "Candle", "Cure-All", "Cursed Skull",
            "Dynamite", "Lantern_Faerie", "FirstAidKit", "Flare", "Heat Pack", "Lantern",
            "PandorasBox", "Parachute", "HealingPuffShroom", "RescueHook",
            "RescueHook_Infinite", "RitualDagger", "ScoutEffigy", "Bugle_Scoutmaster Variant",
            "Sunscreen", "BookOfBones", "Torch", "Warp Compass", "Cheat Compass",
            "Cheat Compass 1", "WarpFungus");
        Add(tags, ItemCategory.Equipment,
            "AncientIdol", "Backpack", "Balloon", "BalloonBunch", "Binoculars_Prop",
            "Binoculars", "Bugle_Prop Variant", "Bugle", "Compass", "Fannypack", "Glider",
            "Jetpack", "Parasol", "Parasol_Roots Variant", "Parachute", "Pirate Compass",
            "Rocketpack");
        Add(tags, ItemCategory.Deployable,
            "RopeShooterAnti", "Anti-Rope Spool", "BounceShroom", "ChainShooter",
            "Flag_Plantable_Checkpoint", "CloudFungus", "MagicBean", "ClimbingSpike",
            "PortableStovetopItem", "RopeShooter", "RopeSpool", "ScoutCannonItem",
            "ShelfShroom");
        Add(tags, ItemCategory.Mystical,
            "AncientIdol", "RopeShooterAnti", "Anti-Rope Spool", "AntiZooka", "Cure-All",
            "Cursed Skull", "Lantern_Faerie", "PandorasBox", "RitualDagger", "ScoutEffigy",
            "Amulet_InfiniteStamina", "Amulet_Clone", "ScoutsHonor", "Amulet_SuperJump",
            "Amulet_Healing", "Bugle_Scoutmaster Variant", "Strange Gem", "BookOfBones",
            "Warp Compass", "Cheat Compass", "Cheat Compass 1");
        return tags;
    }

    private static void Add(
        IDictionary<string, ItemCategory> destination,
        ItemCategory tag,
        params string[] itemNames)
    {
        foreach (string itemName in itemNames)
        {
            destination.TryGetValue(itemName, out ItemCategory existing);
            destination[itemName] = existing | tag;
        }
    }
}
