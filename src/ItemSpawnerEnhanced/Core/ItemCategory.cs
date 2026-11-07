using System;

namespace ItemSpawnerEnhanced.Core;

[Flags]
internal enum ItemCategory
{
    None = 0,
    Food = 1 << 1,
    Consumable = 1 << 2,
    Equipment = 1 << 3,
    Deployable = 1 << 4,
    Mystical = 1 << 5,
    Other = 1 << 6
}
