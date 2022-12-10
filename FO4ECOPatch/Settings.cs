using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace FO4ECOPatch
{
    internal class Settings
    {
        [MaintainOrder] [SettingName("GENERAL")]
        public GeneralSettings GeneralSettings = new();

        [MaintainOrder] [SettingName("ARMOUR")]
        public ArmorSettings ArmorSettings = new();

        [MaintainOrder] [SettingName("WEAPONS")]
        public WeaponSettings WeaponSettings = new();

        [MaintainOrder] [SettingName("LEGENDARY EFFECTS (CHOOSE ONE)")]
        public LegendarySettings LegendarySettings = new();

        [MaintainOrder] [SettingName("CRAFTING RECIPES ")]
        public CraftingSettings CraftingSettings = new();
    }

    internal class GeneralSettings
    {
        [MaintainOrder]
        [SettingName("Skip ECO Records")]
        [Tooltip(
            "Only disable if you know what you're doing. All records contained in ECO's plugin are skipped and won't be modified to ensure that ECO is working as intended.")]
        public bool SkipECORecords = true;
    }

    internal class ArmorSettings
    {
        [MaintainOrder]
        [SettingName("Add missing Legendary Slot")]
        [Tooltip(
            "Add the vanilla legendary attachment point to armour and clothing pieces that don't come with it by default. This enables the use of ECO's slot system on those pieces.")]
        public bool AddLegendary = true;

        [MaintainOrder]
        [SettingName("Add missing INNR")]
        [Tooltip(
            "Add Instance Naming Rules to all eligible armour and clothing pieces. Also fixes Object Templates if necessary.")]
        public bool INNR = true;

        [MaintainOrder]
        [SettingName("Nick can wear Armour")]
        [Tooltip("This option enables Nick Valentine to wear the same armour and clothing the player can wear.")]
        public bool Nick = false;
    }

    internal class WeaponSettings
    {
        [MaintainOrder]
        [SettingName("Add missing Legendary Slot")]
        [Tooltip(
            "Add the vanilla legendary attachment point to ranged and melee weapons that don't come with it by default. This enables the use of ECO's slot system on those pieces.")]
        public bool AddLegendary = true;

        [MaintainOrder]
        [SettingName("Explosives/Throwables Modifier")]
        [Tooltip(
            "This is only called when the prior option is enabled. Add the legendary slot to all sorts of items that can be equipped in the grenade slot: grenades, mines, traps, etc.")]
        public bool Throw = false;

        [MaintainOrder]
        [SettingName("Add missing INNR")]
        [Tooltip(
            "Add Instance Naming Rules to all eligble guns and melee weapons. Also fixes Object Templates if necessary.")]
        public bool INNR = true;
    }

    internal class LegendarySettings
    {
        private bool _addOmodParent = true;
        private bool _setOmodUser = false;

        [MaintainOrder]
        [SettingName("ECO Slots on Mod-Added Legendaries")]
        [Tooltip(
            "Either enable one or none of the mutually exclusive options. Add ECO's primary attachment points (EE,LS,SO) to mod-added/custom legendary effects. It also adds a prefix to the effect that shows on workbenches.")]
        public bool AddOmodParent
        {
            get => _addOmodParent;
            set
            {
                _addOmodParent = value;
                if (_addOmodParent) SetOmodUser = false;
            }
        }

        [MaintainOrder] [SettingName("Prefix")]
        public string AddOmodParentPrefix = "L1: ";

        [MaintainOrder]
        [SettingName("Mod-Added Legendaries in UL Slot")]
        [Tooltip(
            "Either enable one or none of the mutually exclusive options. Move mod-added/custom legendary effects to the separate 'User Legendaries' slot. It also adds a prefix to the effect that shows on workbenches.")]
        public bool SetOmodUser
        {
            get => _setOmodUser;
            set
            {
                _setOmodUser = value;
                if (_setOmodUser) AddOmodParent = false;
            }
        }

        [MaintainOrder] [SettingName("Prefix")]
        public string SetOmodUserPrefix = "UL: ";
    }

    internal class CraftingSettings
    {
        [MaintainOrder] [SettingName("Enable Crafting Recipe Processing")]
        public bool MoveRecipe = true;

        [MaintainOrder] [SettingName("Ignore Vanilla Items")]
        public bool NoVanillaItems = true;

        [MaintainOrder] [SettingName("Only move recipes from Chem Station")]
        public bool FromChemistryStationOnly = true;

        [MaintainOrder] [SettingName("Move Ammo recipes to Ammo Station")]
        public bool Ammo = true;

        [MaintainOrder] [SettingName("Move Armour recipes to Armor Station")]
        public bool Armo = true;

        [MaintainOrder] [SettingName("Move Weapon recipes to Weapon Station")]
        public bool Weap = true;

        [MaintainOrder]
        [SettingName("Move Explosive/Throwable recipes to Weapon Station")]
        [Tooltip("Includes all sorts of items that can be equipped in the grenade slot: grenades, mines, traps, etc.")]
        public bool Expl = true;

        [MaintainOrder]
        [SettingName("Move ALCH (aid,food,...) recipes to Utility Station")]
        [Tooltip("Excludes items from the default HEALING, DRUG and SYRINGER AMMO category.")]
        public bool Alch = false;

        [MaintainOrder]
        [SettingName("Move MISC (mods,junk,..) recipes to Junk and Utility Station respectively")]
        [Tooltip(
            "The recipes for MISC items with components/materials attached to them are transferred to the Junk Station while the rest goes to the Utility Station.")]
        public bool Misc = true;

        [MaintainOrder] [SettingName("Move NOTE (notes, holotapes,..) recipes to Utility Station")]
        public bool Note = true;
    }
}