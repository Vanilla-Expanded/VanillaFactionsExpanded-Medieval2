using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;



namespace VFEMedieval
{


    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public static class VFEMedieval_DefGenerator_GenerateImpliedDefs_PreResolve_Patch
    {
        [HarmonyPrefix]
        public static void GenerateTerrainDefs(bool hotReload = false)
        {

            foreach (TerrainDef item in ImpliedTerrainDefs(hotReload))
            {
                DefGenerator.AddImpliedDef(item, hotReload);
            }

            foreach (ThingDef item2 in ImpliedCobblestoneWallsDefs(hotReload))
            {
                DefGenerator.AddImpliedDef(item2, hotReload);
            }


        }


        public static IEnumerable<TerrainDef> ImpliedTerrainDefs(bool hotReload = false)
        {
            IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where(x => x.thingCategories?.Contains(ThingCategoryDefOf.Leathers) == true);
            foreach (ThingDef def in enumerable)
            {

                int index = 0;
                foreach (LeatherTerrainTemplateDef templateDef in DefDatabase<LeatherTerrainTemplateDef>.AllDefs)
                {
                    yield return TerrainFromLeather(templateDef, def, index, hotReload);
                    index++;
                }
            }
        }

        public static TerrainDef TerrainFromLeather(LeatherTerrainTemplateDef tp, ThingDef def, int index, bool hotReload = false)
        {
            string defName = tp.defName + def.defName;
            TerrainDef terrainDef = (hotReload ? (DefDatabase<TerrainDef>.GetNamed(defName, errorOnFail: false) ?? new TerrainDef()) : new TerrainDef());
            terrainDef.defName = defName;
            terrainDef.label = tp.label.Formatted(def.label);
            terrainDef.texturePath = tp.texturePath;
            terrainDef.researchPrerequisites = tp.researchPrerequisites;
            terrainDef.burnedDef = tp.burnedDef;
            terrainDef.costList = new List<ThingDefCountClass> {
               new ThingDefCountClass
               {
                   thingDef=def,
                   count=tp.costList

               }

            };
            terrainDef.description = tp.description;
            terrainDef.color = def.stuffProps.color;
            terrainDef.designatorDropdown = tp.designatorDropdown;
            terrainDef.uiOrder = tp.uiOrder;
            terrainDef.statBases = tp.statBases;
            terrainDef.renderPrecedence = tp.renderPrecedenceStart - index;
            terrainDef.constructionSkillPrerequisite = tp.constructionSkillPrerequisite;
            terrainDef.canGenerateDefaultDesignator = tp.canGenerateDefaultDesignator;
            terrainDef.tags = tp.tags;
            terrainDef.dominantStyleCategory = tp.dominantStyleCategory;
            terrainDef.layerable = true;
            terrainDef.affordances = new List<TerrainAffordanceDef>
                {
                    TerrainAffordanceDefOf.Light,
                    TerrainAffordanceDefOf.Medium,
                    TerrainAffordanceDefOf.Heavy
                };
            terrainDef.designationCategory = DesignationCategoryDefOf.Floors;
            terrainDef.fertility = 0f;
            terrainDef.constructEffect = EffecterDefOf.ConstructDirt;
            terrainDef.pollutionColor = new Color(1f, 1f, 1f, 0.8f);
            terrainDef.pollutionOverlayScale = new Vector2(0.75f, 0.75f);
            terrainDef.pollutionOverlayTexturePath = "Terrain/Surfaces/PollutionFloorSmooth";
            terrainDef.terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy;
            if (ModsConfig.BiotechActive)
            {
                terrainDef.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearBurn;
            }
            return terrainDef;
        }

        public static IEnumerable<ThingDef> ImpliedCobblestoneWallsDefs(bool hotReload = false)
        {
            List<ThingDef> listOfChunks = DefDatabase<ThingDef>.AllDefs.Where(x => x.thingCategories?.Contains(ThingCategoryDefOf.StoneChunks) == true).ToList();

            foreach (ThingDef def2 in listOfChunks)
            {

                int index = 0;
                foreach (CobblestoneWallTemplateDef templateDef2 in DefDatabase<CobblestoneWallTemplateDef>.AllDefs.ToList())
                {
                    yield return CobblestoneWallFromChunk(templateDef2, def2, index, hotReload);
                    index++;
                }
            }

        }

        public static ThingDef CobblestoneWallFromChunk(CobblestoneWallTemplateDef tp, ThingDef def, int index, bool hotReload = false)
        {
            string defName = tp.defName + def.defName;
            ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? new ThingDef()) : new ThingDef());
            thingDef.defName = defName;
            thingDef.label = tp.label.Formatted(def.label);
            thingDef.uiIconColor = def.graphicData.color;
            thingDef.graphicData = new GraphicData()
            {
                texPath = tp.texPath,
                graphicClass = tp.graphicClass,
                linkType = tp.linkType,
                linkFlags = tp.linkFlags,
                damageData = tp.damageData,
                color = def.graphicData.color

            };

            thingDef.category = tp.category;
            thingDef.thingClass = tp.thingClass;
            thingDef.soundImpactDefault = tp.soundImpactDefault;
            thingDef.drawerType = tp.drawerType;
            thingDef.repairEffect = tp.repairEffect;
            thingDef.filthLeaving = tp.filthLeaving;
            thingDef.uiIconPath = tp.uiIconPath;

            thingDef.building = tp.building;
            thingDef.leaveResourcesWhenKilled = tp.leaveResourcesWhenKilled;
            thingDef.altitudeLayer = tp.altitudeLayer;
            thingDef.passability = tp.passability;
            thingDef.blockWind = tp.blockWind;
            thingDef.castEdgeShadows = tp.castEdgeShadows;
            thingDef.fillPercent = tp.fillPercent;
            thingDef.coversFloor = tp.coversFloor;
            thingDef.placingDraggableDimensions = tp.placingDraggableDimensions;
            thingDef.tickerType = tp.tickerType;
            thingDef.rotatable = tp.rotatable;

            thingDef.selectable = tp.selectable;
            thingDef.neverMultiSelect = tp.neverMultiSelect;
            thingDef.useStuffTerrainAffordance = tp.useStuffTerrainAffordance;
            thingDef.terrainAffordanceNeeded = tp.terrainAffordanceNeeded;
            thingDef.holdsRoof = tp.holdsRoof;
            thingDef.designationCategory = tp.designationCategory;
            thingDef.staticSunShadowHeight = tp.staticSunShadowHeight;
            thingDef.blockLight = tp.blockLight;

            thingDef.fertility = tp.fertility;

            thingDef.stuffCategories = tp.stuffCategories;
            thingDef.comps = tp.comps;
            thingDef.damageMultipliers = tp.damageMultipliers;
            thingDef.designatorDropdown = tp.designatorDropdown;
            thingDef.costList = new List<ThingDefCountClass> {
               new ThingDefCountClass
               {
                   thingDef=def,
                   count=1

               }

};
            ThingDef blocksToGrab = def.butcherProducts?.First()?.thingDef;

            thingDef.statBases = new List<StatModifier> {

                new StatModifier
               {
                   stat=StatDefOf.MaxHitPoints,
                   value= (blocksToGrab?.stuffProps!=null) ? 300 * blocksToGrab.stuffProps.statFactors.Where(x => x.stat == StatDefOf.MaxHitPoints).Select(x => x.value).First() : 300


               },
                 new StatModifier
               {
                   stat=StatDefOf.WorkToBuild,
                   value= (blocksToGrab?.stuffProps!=null) ? 135 * blocksToGrab.stuffProps.statFactors.Where(x => x.stat == StatDefOf.WorkToBuild).Select(x => x.value).First() : 135

               }
                 ,
                 new StatModifier
               {
                   stat=StatDefOf.Flammability,
                   value= 1

               } ,
                 new StatModifier
               {
                   stat=StatDefOf.MeditationFocusStrength,
                   value= 0.22f

               },
                 new StatModifier
               {
                   stat=StatDefOf.Beauty,
                   value= -2

               },
                 new StatModifier
               {
                   stat=StatDefOf.SellPriceFactor,
                   value= 0.7f

               }


            };











            return thingDef;
        }

    }








}
