using GraphicCustomization;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using VFECore;
using static RimWorld.Dialog_StylingStation;

namespace VFEMedieval
{
    public static class Heraldic
    {
        [DebugAction("VFEMedieval", "Edit Heraldics For Selected", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void EditHeraldicsForSelected()
        {
            var thing = Find.Selector.SelectedObjects.OfType<Pawn>().FirstOrDefault();
            if (thing == null) Find.Selector.SelectedObjects.OfType<Thing>().FirstOrDefault();
            if (thing == null) throw new Exception("No valid thing selected for Heraldic editing.");
            var window = new Dialog_Heraldic(thing);
            Find.WindowStack.Add(window);
        }

        public const string MASK_CLR_A = "HeraldryColorA";
        public const string MASK_CLR_B = "HeraldryColorB";
        public const string MASK_PATH = "HeraldryPattern";
        public const string SYMBOL_CLR = "HeraldrySymbolColor";
        public const string SYMBOL_PATH = "HeraldrySymbol";
    }

    public class HeraldicManager : GameComponent
    {
        public static List<HeraldicPattern> allPatterns = new List<HeraldicPattern>();
        public static List<HeraldicSymbol> allSymbols = new List<HeraldicSymbol>();

        public HeraldicManager(Game game) { }

        public static void SetupHeraldics()
        {
            allPatterns = DefDatabase<HeraldicPattern>.AllDefsListForReading;
            allSymbols = DefDatabase<HeraldicSymbol>.AllDefsListForReading;

            var allFactions = Find.FactionManager.AllFactionsListForReading;
            foreach(var faction in allFactions)
            {
                if (faction.temporary)
                {
                    continue;
                }
                new HeraldicSettings(faction);
            }
        }

        public override void FinalizeInit()
        {
            SetupHeraldics();
        }
    }

    public abstract class HeraldicBase : Def
    {
        public string path;
        public float weight = 1;
        public bool blankType = false;
    }
    public class HeraldicPattern : HeraldicBase { };
    public class HeraldicSymbol : HeraldicBase { };


    /// <summary>
    /// Used by the Heraldic Customizer, never saved. Regenerated each time.
    /// </summary>
    public class HeraldicSettings
    {
        public Faction faction;
        public Thing thing = null;

        public TaggedText maskPath = null;
        public TaggedColor maskColorA = null;
        public TaggedColor maskColorB = null;

        public TaggedText symbolPath = null;
        public TaggedColor symbolColor = null;

        public List<(string, ITaggedItem)> Items => new List<(string, ITaggedItem)>
        {
            (Heraldic.SYMBOL_PATH, symbolPath),
            (Heraldic.SYMBOL_CLR, symbolColor),
            (Heraldic.MASK_PATH, maskPath),
            (Heraldic.MASK_CLR_B, maskColorA),
            (Heraldic.MASK_CLR_B, maskColorB),
        };

        // Used if we need defaults for some reason.
        public HeraldicSettings()
        {
            symbolColor = new TaggedColor(Heraldic.SYMBOL_CLR, new Color(0.15f, 0.15f, 0.15f, 1f));
            maskColorA = new TaggedColor(Heraldic.MASK_CLR_A, new(0.45f, 0.2f, 0.2f));
            maskColorB = new TaggedColor(Heraldic.MASK_CLR_B, new(0.15f, 0.28f, 0.43f));
            maskPath = new TaggedText(Heraldic.MASK_PATH, HeraldicManager.allPatterns.First().path);
            symbolPath = new TaggedText(Heraldic.SYMBOL_PATH, HeraldicManager.allSymbols.First().path);
        }

        public HeraldicSettings(Thing thing)
        {
            this.thing = thing;
            Refresh();
        }

        public HeraldicSettings(Faction faction)
        {
            this.faction = faction;
            Refresh();

            // Note that these are just defaults if a faction doesn't define any heraldic settings.
            if (maskColorA == null || maskColorB == null || maskPath == null || symbolColor == null || symbolPath == null)
            {
                int factionTexSeed = 0;
                factionTexSeed += faction.GetUniqueLoadID().GetHashCode();
                if (faction.ideos?.PrimaryIdeo is Ideo ideo)
                {
                    factionTexSeed += ideo.GetUniqueLoadID().GetHashCode();
                }

                using (new RandBlock(factionTexSeed))
                {
                    var pattern = HeraldicManager.allPatterns.RandomElementByWeight(x => x.weight);
                    maskPath = new TaggedText(Heraldic.MASK_CLR_A, pattern.path);

                    symbolPath = new TaggedText(Heraldic.MASK_CLR_B,
                        HeraldicManager.allSymbols
                            .Where(x => !(x.blankType && pattern.blankType))  // Prevent double-blank.
                            .RandomElementByWeight(x => x.weight).path);
                }
                var aColorA = new AdvancedColor { primaryFactionIdeoColor = true };
                var aColorB = new AdvancedColor
                {
                    primaryFactionIdeoColor = true,
                    hueRotate = -0.41f,
                    invertValueIfBelow = 0.5f,
                    invertValueIfAbove = 0.8f,
                    minBrightness = 0.35f
                };

                maskColorA = new TaggedColor(Heraldic.MASK_CLR_A, aColorA.GetColor(faction));
                maskColorB = new TaggedColor(Heraldic.MASK_CLR_B, aColorB.GetColor(faction));
                symbolColor = new TaggedColor(Heraldic.SYMBOL_CLR, new Color(0.15f, 0.15f, 0.15f, 1f));
                SaveTagsTo(faction);
            }
        }

        public bool HasAllTags()
        {
            return maskColorA != null && maskColorB != null && maskPath != null && symbolColor != null && symbolPath != null;
        }

        public void Refresh()
        {
            if (thing != null)
            {
                // Used to reload the data after making changes.
                maskColorA = thing.GetColorByTag(Heraldic.MASK_CLR_A);
                maskColorB = thing.GetColorByTag(Heraldic.MASK_CLR_B);
                maskPath = thing.GetStringByTag(Heraldic.MASK_PATH);
                symbolColor = thing.GetColorByTag(Heraldic.SYMBOL_CLR);
                symbolPath = thing.GetStringByTag(Heraldic.SYMBOL_PATH);
            }
            else if (faction != null)
            {
                RegenerateFaction(faction);
            }
        }

        private void RegenerateFaction(Faction faction)
        {
            maskColorA = faction.GetColorByTag(Heraldic.MASK_CLR_A);
            maskColorB = faction.GetColorByTag(Heraldic.MASK_CLR_B);
            maskPath = faction.GetStringByTag(Heraldic.MASK_PATH);
            symbolColor = faction.GetColorByTag(Heraldic.SYMBOL_CLR);
            symbolPath = faction.GetStringByTag(Heraldic.SYMBOL_PATH);
        }

        public void SaveTagsTo(ILoadReferenceable tar)
        {
            TaggedColorPropManager.SetTagItem(tar, new TaggedColor(Heraldic.MASK_CLR_A, maskColorA.value));
            TaggedColorPropManager.SetTagItem(tar, new TaggedColor(Heraldic.MASK_CLR_B, maskColorB.value));
            TaggedTextPropManager.SetTagItem(tar, new TaggedText(Heraldic.MASK_PATH, maskPath.value));
            TaggedColorPropManager.SetTagItem(tar, new TaggedColor(Heraldic.SYMBOL_CLR, symbolColor.value));
            TaggedTextPropManager.SetTagItem(tar, new TaggedText(Heraldic.SYMBOL_PATH, symbolPath.value));
        }
    }
}
