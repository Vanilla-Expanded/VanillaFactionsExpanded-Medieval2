using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    public static class ResearchManager_FinishProject_Patch
    {
        public static void Prefix(ResearchProjectDef proj)
        {
            ResearchUtility.TryChangeBaseCost(proj);
        }

        public static void Postfix(ResearchProjectDef proj)
        {
            if (Find.Storyteller.def == VFEM_DefOf.VFEM_MaynardMedieval && proj.baseCost > 0)
            {
                ResearchManager_ExposeData_Patch.finishedCosts[proj] = proj.baseCost;
            }
        }
    }
}