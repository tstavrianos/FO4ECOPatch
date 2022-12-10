// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.Fallout4;

public static partial class ECO
{
    public static class Holotape
    {
        private static FormLink<IHolotapeGetter> Construct(uint id) => new FormLink<IHolotapeGetter>(ModKey.MakeFormKey(id));
        public static FormLink<IHolotapeGetter> Dank_Holotape_Config => Construct(0x949);
    }
}
