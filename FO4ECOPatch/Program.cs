﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.FormKeys.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Noggog;

namespace FO4ECOPatch
{
    public static class Program
    {
        private static Lazy<Settings> _settings;
        private static Settings Settings => _settings.Value;

        private static readonly HashSet<ModKey> Vanilla = new()
        {
            ModKey.FromNameAndExtension("Fallout4.esp"),
            ModKey.FromNameAndExtension("DLCRobot.esl"),
            ModKey.FromNameAndExtension("DLCworkshop01.esp"),
            ModKey.FromNameAndExtension("DLCCoast.esp"),
            ModKey.FromNameAndExtension("DLCworkshop02.esp"),
            ModKey.FromNameAndExtension("DLCworkshop03.esp"),
            ModKey.FromNameAndExtension("DLCNukaWorld.esp")
        };

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetAutogeneratedSettings("Settings", "settings.json", out _settings)
                .SetTypicalOpen(GameRelease.Fallout4, "ECO_Patch.esp")
                .AddRunnabilityCheck(state =>
                {
                    state.LoadOrder.AssertListsMod(ECO.ModKey,
                        "\nECO plugin missing, not active, or inaccessible to patcher!\n\n");
                })
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state)
        {
            if (Settings.ArmorSettings.Nick)
                foreach (var armorAddonContext in state.LoadOrder.PriorityOrder.ArmorAddon().WinningContextOverrides())
                {
                    if (Settings.GeneralSettings.SkipECORecords && armorAddonContext.ModKey == ECO.ModKey) continue;

                    if (armorAddonContext.Record.IsDeleted) continue;
                    var record = armorAddonContext.Record;

                    if (string.IsNullOrWhiteSpace(record.EditorID)) continue;
                    if (record.EditorID.Contains("Naked", StringComparison.OrdinalIgnoreCase)) continue;
                    if (record.BodyTemplate is not null)
                    {
                        if (record.BodyTemplate.FirstPersonFlags.HasFlag(BodyTemplate.Flag.Scalp) ||
                            record.BodyTemplate.FirstPersonFlags.HasFlag(BodyTemplate.Flag.Decapitation)) continue;
                        if (record.BodyTemplate.FirstPersonFlags is BipedObjectFlag.FaceGenHead
                            or BipedObjectFlag.Pipboy or BipedObjectFlag.FX) continue;
                    }

                    var found = false;
                    foreach (var additionalRace in record.AdditionalRaces)
                        if (additionalRace.FormKey == Fallout4.Race.SynthGen2RaceValentine.FormKey)
                        {
                            found = true;
                            break;
                        }

                    if (found) continue;
                    if (record.Race.FormKey == Fallout4.Race.HumanRace.FormKey)
                    {
                        var newRecord = armorAddonContext.GetOrAddAsOverride(state.PatchMod);
                        newRecord.AdditionalRaces.Add(Fallout4.Race.SynthGen2RaceValentine);
                    }
                }

            if (Settings.ArmorSettings.AddLegendary || Settings.ArmorSettings.INNR)
                foreach (var armorContext in state.LoadOrder.PriorityOrder.Armor().WinningContextOverrides())
                {
                    if (Settings.GeneralSettings.SkipECORecords && armorContext.ModKey == ECO.ModKey) continue;
                    if (armorContext.Record is null) continue;
                    if (armorContext.Record.IsDeleted) continue;
                    var record = armorContext.Record;

                    if (record.MajorFlags.HasFlag(Armor.MajorFlag.NonPlayable)) continue;
                    if (record.Name is null || string.IsNullOrWhiteSpace(record.Name.String)) continue;
                    if (record.Race.FormKey != Fallout4.Race.HumanRace.FormKey) continue;

                    //IArmor newRecord = null;
                    var newRecord = record.DeepCopy();
                    var modified = false;
                    if (Settings.ArmorSettings.AddLegendary)
                        if (!record.HasKeyword(Fallout4.Keyword.ap_Legendary))
                        {
                            if (newRecord.Keywords is null)
                                newRecord.Keywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>();

                            newRecord.Keywords.Add(Fallout4.Keyword.ap_Legendary);
                            modified = true;
                        }

                    if (Settings.ArmorSettings.INNR)
                    {
                        var changedInnr = false;
                        var needToChange = record.InstanceNaming.IsNull ||
                                           string.IsNullOrEmpty(record.InstanceNaming.TryResolve(state.LinkCache)
                                               .EditorID);
                        if (!record.HasKeyword(Fallout4.Keyword.ArmorTypePower))
                        {
                            if (needToChange)
                            {
                                newRecord.InstanceNaming.SetTo(Fallout4.InstanceNamingRules.dn_CommonArmor);
                                changedInnr = true;
                                modified = true;
                            }
                        }
                        else
                        {
                            if (needToChange)
                            {
                                newRecord.InstanceNaming.SetTo(Fallout4.InstanceNamingRules.dn_PowerArmor);
                                changedInnr = true;
                                modified = true;
                            }
                        }

                        if (changedInnr || needToChange)
                            if (record.ObjectTemplates is null || record.ObjectTemplates.Count == 0)
                            {
                                if (newRecord.ObjectTemplates is null)
                                    newRecord.ObjectTemplates = new ExtendedList<ObjectTemplate<Armor.Property>>();
                                var obte = new ObjectTemplate<Armor.Property>();
                                obte.AddonIndex = -1;
                                obte.Default = true;
                                newRecord.ObjectTemplates.Add(obte);
                                modified = true;
                            }
                    }

                    if (modified) state.PatchMod.Armors.Set(newRecord);
                }

            if (Settings.WeaponSettings.AddLegendary || Settings.WeaponSettings.INNR || Settings.WeaponSettings.Throw)
                foreach (var weaponContext in state.LoadOrder.PriorityOrder.Weapon().WinningContextOverrides())
                {
                    if (Settings.GeneralSettings.SkipECORecords && weaponContext.ModKey == ECO.ModKey) continue;
                    if (weaponContext.Record is null) continue;
                    if (weaponContext.Record.IsDeleted) continue;

                    if (weaponContext.Record.FormKey.ModKey ==
                        ModKey.FromNameAndExtension("IDEKsLogisticsStation2.esl")) continue;

                    var record = weaponContext.Record;
                    if (record.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)) continue;
                    if (record.Flags.HasFlag(Weapon.Flag.NotPlayable)) continue;
                    if (record.Name is null || string.IsNullOrWhiteSpace(record.Name.String)) continue;

                    if (record.Keywords is null) continue;

                    if (record.Flags.HasFlag(Weapon.Flag.NotUsedInNormalCombat) ||
                        record.Flags.HasFlag(Weapon.Flag.NonHostile) || record.Flags.HasFlag(Weapon.Flag.CannotDrop)
                        || record.Flags.HasFlag(Weapon.Flag.EmbeddedWeapon)) continue;

                    if (record.EquipmentType.IsNull ||
                        !(record.EquipmentType.FormKey == Fallout4.EquipType.BothHands.FormKey ||
                          record.EquipmentType.FormKey == Fallout4.EquipType.BothHandsLeftOptional.FormKey ||
                          record.EquipmentType.FormKey == Fallout4.EquipType.GrenadeSlot.FormKey ||
                          record.EquipmentType.FormKey == Fallout4.EquipType.RightHand.FormKey))
                        continue;
                    if (!Settings.WeaponSettings.Throw &&
                        record.EquipmentType.FormKey == Fallout4.EquipType.GrenadeSlot.FormKey) continue;

                    var newRecord = record.DeepCopy();
                    var modified = false;
                    if (Settings.WeaponSettings.AddLegendary && !record.HasKeyword(Fallout4.Keyword.ap_Legendary))
                    {
                        if (newRecord.Keywords is null)
                            newRecord.Keywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>();
                        newRecord.Keywords.Add(Fallout4.Keyword.ap_Legendary);
                        modified = true;
                    }

                    if (Settings.WeaponSettings.INNR)
                    {
                        var changedInnr = false;
                        var needToChange = record.InstanceNaming.IsNull ||
                                           string.IsNullOrEmpty(record.InstanceNaming.TryResolve(state.LinkCache)
                                               .EditorID);
                        if (needToChange)
                        {
                            if (record.AnimationType == Weapon.AnimationTypes.Gun)
                            {
                                newRecord.InstanceNaming.SetTo(Fallout4.InstanceNamingRules.dn_CommonGun);
                                changedInnr = true;
                                modified = true;
                            }
                            else if (record.AnimationType is Weapon.AnimationTypes.OneHandAxe
                                     or Weapon.AnimationTypes.OneHandDagger or Weapon.AnimationTypes.OneHandSword
                                     or Weapon.AnimationTypes.TwoHandAxe or Weapon.AnimationTypes.TwoHandSword)
                            {
                                newRecord.InstanceNaming.SetTo(Fallout4.InstanceNamingRules.dn_CommonMelee);
                                changedInnr = true;
                                modified = true;
                            }
                        }

                        if (changedInnr || needToChange)
                            if (record.ObjectTemplates is null || record.ObjectTemplates.Count == 0)
                            {
                                if (newRecord.ObjectTemplates is null)
                                    newRecord.ObjectTemplates = new ExtendedList<ObjectTemplate<Weapon.Property>>();
                                var obte = new ObjectTemplate<Weapon.Property>();
                                obte.AddonIndex = -1;
                                obte.Default = true;
                                newRecord.ObjectTemplates.Add(obte);
                                modified = true;
                            }
                    }

                    if (modified) state.PatchMod.Weapons.Set(newRecord);
                }

            if (Settings.LegendarySettings.AddOmodParent || Settings.LegendarySettings.SetOmodUser)
                foreach (var omodContext in state.LoadOrder.PriorityOrder.AObjectModification()
                             .WinningContextOverrides())
                {
                    if (Settings.GeneralSettings.SkipECORecords && omodContext.ModKey == ECO.ModKey) continue;
                    if (omodContext.Record is null) continue;
                    if (omodContext.Record.IsDeleted) continue;

                    if (omodContext.Record.FormKey.ModKey ==
                        ModKey.FromNameAndExtension("IDEKsLogisticsStation2.esl")) continue;

                    var record = omodContext.Record;
                    if (record.AttachPoint.FormKey != Fallout4.Keyword.ap_Legendary.FormKey) continue;

                    var newRecord = record.DeepCopy();
                    var modified = false;
                    if (Settings.LegendarySettings.AddOmodParent && !Settings.LegendarySettings.SetOmodUser)
                    {
                        var found = false;
                        foreach (var attachParentSlot in record.AttachParentSlots)
                            if (attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Unscrap.FormKey)
                            {
                                found = true;
                                break;
                            }

                        if (!found)
                        {
                            newRecord.AttachParentSlots.Add(ECO.Keyword.Dank_ap_Unscrap);
                            modified = true;
                        }

                        found = false;
                        foreach (var attachParentSlot in record.AttachParentSlots)
                            if (attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Special_Slot.FormKey)
                            {
                                found = true;
                                break;
                            }

                        if (!found)
                        {
                            newRecord.AttachParentSlots.Add(ECO.Keyword.Dank_ap_Special_Slot);
                            modified = true;
                        }

                        found = false;
                        foreach (var attachParentSlot in record.AttachParentSlots)
                            if (attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Legendary_Slots.FormKey)
                            {
                                found = true;
                                break;
                            }

                        if (!found)
                        {
                            newRecord.AttachParentSlots.Add(ECO.Keyword.Dank_ap_Legendary_Slots);
                            modified = true;
                        }

                        if (record.Name is not null && !string.IsNullOrEmpty(record.Name.String)
                                                    && !record.Name.String.StartsWith("L1",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L2",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L3",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L4",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L5",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("UL",
                                                        StringComparison.OrdinalIgnoreCase)
                           )
                        {
                            newRecord.Name = Settings.LegendarySettings.AddOmodParentPrefix + newRecord.Name;
                            modified = true;
                        }
                    }

                    if (!Settings.LegendarySettings.AddOmodParent && Settings.LegendarySettings.SetOmodUser)
                    {
                        var found = false;
                        foreach (var attachParentSlot in record.AttachParentSlots)
                            if (attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Unscrap.FormKey
                                || attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Special_Slot.FormKey
                                || attachParentSlot.FormKey == ECO.Keyword.Dank_ap_Legendary_Slots.FormKey)
                            {
                                found = true;
                                break;
                            }

                        if (!found)
                        {
                            newRecord.AttachPoint.SetTo(ECO.Keyword.Dank_ap_Legendary_User);
                            modified = true;
                        }

                        if (record.Name is not null && !string.IsNullOrEmpty(record.Name.String)
                                                    && !record.Name.String.StartsWith("L1",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L2",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L3",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L4",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("L5",
                                                        StringComparison.OrdinalIgnoreCase)
                                                    && !record.Name.String.StartsWith("UL",
                                                        StringComparison.OrdinalIgnoreCase)
                           )
                        {
                            newRecord.Name = Settings.LegendarySettings.SetOmodUserPrefix + newRecord.Name;
                            modified = true;
                        }
                    }

                    if (modified) state.PatchMod.ObjectModifications.Set(newRecord);
                }

            if (Settings.CraftingSettings.MoveRecipe)
                foreach (var cobjContext in state.LoadOrder.PriorityOrder.ConstructibleObject()
                             .WinningContextOverrides())
                {
                    if (Settings.GeneralSettings.SkipECORecords && cobjContext.ModKey == ECO.ModKey) continue;
                    if (Settings.CraftingSettings.NoVanillaItems && Vanilla.Contains(cobjContext.ModKey)) continue;
                    if (cobjContext.Record is null) continue;
                    if (cobjContext.Record.IsDeleted) continue;

                    if (cobjContext.Record.FormKey.ModKey ==
                        ModKey.FromNameAndExtension("IDEKsLogisticsStation2.esl")) continue;

                    var record = cobjContext.Record;
                    if (record.WorkbenchKeyword.IsNull) continue;

                    if (record.CreatedObject.IsNull) continue;

                    if (Settings.CraftingSettings.FromChemistryStationOnly && record.WorkbenchKeyword.FormKey !=
                        Fallout4.Keyword.WorkbenchChemlab.FormKey)
                        continue;

                    var craftRes = record.CreatedObject.TryResolve(state.LinkCache);
                    if (craftRes is null || craftRes.IsDeleted) continue;

                    FormLink<IKeywordGetter> target = null;

                    if (Settings.CraftingSettings.Ammo && craftRes is IAmmunitionGetter)
                    {
                        target = ECO.Keyword.Dank_Workbench_TypeAmmo;
                    }
                    else if (Settings.CraftingSettings.Armo && craftRes is IArmorGetter)
                    {
                        target = ECO.Keyword.Dank_Workbench_TypeArmorCreate;
                    }
                    else if ((Settings.CraftingSettings.Weap || Settings.CraftingSettings.Expl) &&
                             craftRes is IWeaponGetter weapon)
                    {
                        if (Settings.CraftingSettings.Weap &&
                            weapon.AnimationType != Weapon.AnimationTypes.Grenade &&
                            weapon.AnimationType != Weapon.AnimationTypes.Grenade)
                            target = ECO.Keyword.Dank_Workbench_TypeWeaponCreate;
                        else if (Settings.CraftingSettings.Expl &&
                                 weapon.AnimationType is Weapon.AnimationTypes.Grenade or Weapon.AnimationTypes.Grenade)
                            target = ECO.Keyword.Dank_Workbench_TypeWeaponCreate;
                    }
                    else if (Settings.CraftingSettings.Alch && craftRes is IIngestibleGetter)
                    {
                        target = ECO.Keyword.Dank_Workbench_TypeUtility;
                    }
                    else if (Settings.CraftingSettings.Note && craftRes is IBookGetter or IHolotapeGetter)
                    {
                        target = ECO.Keyword.Dank_Workbench_TypeUtility;
                    }
                    else if (Settings.CraftingSettings.Misc && craftRes is IMiscItemGetter misc)
                    {
                        if (misc.Components is {Count: > 0})
                            target = ECO.Keyword.Dank_Workbench_TypeJunk;
                        else
                            target = ECO.Keyword.Dank_Workbench_TypeUtility;
                    }

                    if (target != null && target.FormKey != record.WorkbenchKeyword.FormKey)
                    {
                        var copy = state.PatchMod.ConstructibleObjects.GetOrAddAsOverride(record);
                        copy.WorkbenchKeyword.SetTo(target);
                    }
                }
        }
    }
}