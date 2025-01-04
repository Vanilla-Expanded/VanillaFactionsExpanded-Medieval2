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


    }








}
