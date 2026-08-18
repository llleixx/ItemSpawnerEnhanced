using System;
using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Core;

internal static class VanillaItemVisibility
{
    private static readonly HashSet<string> HiddenInternalNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        // Documented by the PEAK wiki as unused or unavailable in normal gameplay.
        "ClimbingChalk",
        "Clusterberry_UNUSED",
        "FireWood",
        "GuidebookPage_13_FirstTeams",
        "Megaphone",
        "Mushroom Glow",
        "Stone",
        "Cheat Compass",
        "Cheat Compass 1",

        // Editor/test variants that duplicate a playable item or contain placeholder UI data.
        "BingBong_Prop Variant",
        "Binoculars_Prop",
        "Bugle_Prop Variant",
        "GuidebookPage",
        "Lollipop_Prop",
        "Mandrake_Hidden",
        "Parasol_Roots Variant",
        "ScoutmasterSoul",
        "Skull",
        "Warpsketball",
        "C_Bishop_f Variant",
        "C_Bishop_f",
        "C_Bishop_m Variant",
        "C_Bishop_m",
        "C_King Variant",
        "C_King",
        "C_Knight Variant",
        "C_Knight",
        "C_Pawn_f Variant",
        "C_Pawn_f",
        "C_Pawn_m Variant",
        "C_Pawn_m",
        "C_Queen Variant",
        "C_Queen",
        "C_Rook_f Variant",
        "C_Rook_f",
        "C_Rook_m Variant",
        "C_Rook_m"
    };

    public static IReadOnlyCollection<string> DefaultHiddenInternalNames => HiddenInternalNames;

    public static bool IsVisible(string internalName, bool showAllItems, bool isFavorite) =>
        showAllItems || isFavorite || !IsDefaultHidden(internalName);

    public static bool IsDefaultHidden(string internalName) => HiddenInternalNames.Contains(internalName);
}
