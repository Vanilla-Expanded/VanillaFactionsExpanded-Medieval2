using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    public static class ResearchManager_FinishProject_Patch
    {
        public static void Prefix(ResearchProjectDef proj, out float __state)
        {
            __state = proj.baseCost;
            if (proj.baseCost > 0)
            {
                if (ResearchManager_ExposeData_Patch.finishedCosts != null && ResearchManager_ExposeData_Patch.finishedCosts.TryGetValue(proj, out float value))
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
                    proj.baseCost = Mathf.Round(proj.baseCost);
                }
            }
        }

        public static void Postfix(ResearchProjectDef proj, float __state)
        {
            if (Find.Storyteller.def == VFEM_DefOf.VFEM_MaynardMedieval && proj.baseCost > 0)
            {
                ResearchManager_ExposeData_Patch.finishedCosts[proj] = proj.baseCost;
            }
            proj.baseCost = __state;
        }
    }
}