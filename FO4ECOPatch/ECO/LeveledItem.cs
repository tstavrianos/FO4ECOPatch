// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.Fallout4;

public static partial class ECO
{
    public static class LeveledItem
    {
        private static FormLink<ILeveledItemGetter> Construct(uint id) => new FormLink<ILeveledItemGetter>(ModKey.MakeFormKey(id));
        public static FormLink<ILeveledItemGetter> Dank_LL_Microchip => Construct(0x860);
    }
}
