using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    public static class ResearchManager_FinishProject_Patch
    {
        public static Dictionary<ResearchProjectDef, float> finishedCosts = new Dictionary<ResearchProjectDef, float>();
        public static void Prefix(ResearchProjectDef proj, out float __state)
        {
            __state = proj.baseCost;
            if (finishedCosts.TryGetValue(proj, out float value))
            {
                proj.baseCost = value;
            }
            else if (Find.Storyteller.def == VFEM_DefOf.VFEM_MaynardMedieval && proj.baseCost > 0 && proj.ProgressReal < proj.baseCost)
            {
                int completedProjectsCount = 0;
                foreach (ResearchProjectDef def in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                {
                    if (def.baseCost > 0 && def.ProgressReal >= def.baseCost)
                    {
                        completedProjectsCount++;
                    }
                }
                float researchCostModifier = 1f + (completedProjectsCount * 0.01f);
                proj.baseCost *= researchCostModifier;
            }
        }

        public static void Postfix(ResearchProjectDef proj, float __state)
        {
            finishedCosts[proj] = proj.baseCost;
            proj.baseCost = __state;
        }
    }
}