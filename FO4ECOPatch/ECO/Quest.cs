// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.Fallout4;

public static partial class ECO
{
    public static class Quest
    {
        private static FormLink<IQuestGetter> Construct(uint id) => new FormLink<IQuestGetter>(ModKey.MakeFormKey(id));
        public static FormLink<IQuestGetter> Dank_QGM_ModSwitcher => Construct(0xaa2);
        public static FormLink<IQuestGetter> Dank_AddLegendaryItemQuest_AIO => Construct(0xe84);
        public static FormLink<IQuestGetter> Dank_AddLegendaryItemQuest_Custom => Construct(0xe85);
        public static FormLink<IQuestGetter> Dank_AddLegendaryItemQuest_FO76 => Construct(0xe86);
        public static FormLink<IQuestGetter> Dank_InjectorStartQuest => Construct(0xe87);
        public static FormLink<IQuestGetter> Dank_DropBenchScript => Construct(0xa8035);
        public static FormLink<IQuestGetter> Dank_ECO_InjectDemo => Construct(0xb9327);
        public static FormLink<IQuestGetter> Dank_MCMfunctions => Construct(0xca2dc);
    }
}