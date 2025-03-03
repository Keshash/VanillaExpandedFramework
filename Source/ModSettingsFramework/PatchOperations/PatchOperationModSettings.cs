﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public abstract class PatchOperationModSettings : PatchOperation
    {
        public int order;
        public string category;
        public List<string> mods;
        public string id;
        public string label;
        public string tooltip;
        public bool showTooltipAsTinyText;
        public string modPackageSettingsID;
        public int roundToDecimalPlaces = 2;

        public ModSettingsContainer SettingsContainer
        {
            get
            {
                if (modPackageSettingsID.NullOrEmpty() is false)
                {
                    var modHandle = LoadedModManager.RunningMods.FirstOrDefault(x => x.PackageIdPlayerFacing.ToLower() 
                    == modPackageSettingsID.ToLower());
                    if (modHandle != null)
                    {
                        return ModSettingsFrameworkSettings.GetModSettingsContainer(modHandle);
                    }
                }
                foreach (var runningMod in LoadedModManager.RunningMods)
                {
                    if (runningMod.Patches.Contains(this))
                    {
                        var container = ModSettingsFrameworkSettings.GetModSettingsContainer(runningMod);
                        return container;
                    }
                }
                return null;
            }
        }
        public abstract void DoSettings(ModSettingsContainer container, Listing_Standard list);
        public virtual int SettingsHeight() => (int)scrollHeight;
        public bool CanRun()
        {
            var modsToCheck = new List<string>();
            if (category.NullOrEmpty() is false)
            {
                var def = DefDatabase<ModOptionCategoryDef>.GetNamedSilentFail(category);
                if (def != null && def.mods != null)
                {
                    modsToCheck.AddRange(def.mods);
                }
            }
            if (mods != null)
            {
                modsToCheck.AddRange(mods);
            }

            if (modsToCheck.NullOrEmpty() is false)
            {
                for (int i = 0; i < modsToCheck.Count; i++)
                {
                    if (ModLister.HasActiveModWithName(modsToCheck[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public float scrollHeight = 99999999;

        protected void DoCheckbox(Listing_Standard listingStandard, string optionLabel, ref bool field, string explanation)
        {
            Rect rect = new Rect(listingStandard.curX, listingStandard.curY, listingStandard.ColumnWidth, Text.LineHeight);
            listingStandard.CheckboxLabeled(optionLabel, ref field);
            ShowExplanation(listingStandard, explanation, rect);
            listingStandard.Gap(5);
            scrollHeight += 29;
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            value = Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            value = (float)Math.Round(value, roundToDecimalPlaces);
            listingStandard.Gap(5);
            scrollHeight += 5;
            ShowExplanation(listingStandard, explanation, rect.LeftPart(0.4f));
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref int value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            value = (int)Widgets.HorizontalSlider_NewTemp(sliderRect, value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            scrollHeight += 5;
            ShowExplanation(listingStandard, explanation, rect.LeftPart(0.4f));
        }

        protected void DoExplanation(Listing_Standard listingStandard, string explanation)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            var rect = listingStandard.Label(explanation);
            scrollHeight += rect.height;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listingStandard.Gap(5);
            scrollHeight += 5;
        }

        private void ShowExplanation(Listing_Standard listingStandard, string explanation, Rect rect)
        {
            if (explanation.NullOrEmpty() is false)
            {
                if (showTooltipAsTinyText)
                {
                    DoExplanation(listingStandard, explanation);
                }
                else
                {
                    TooltipHandler.TipRegion(rect, explanation);
                }
            }
        }
    }
}
