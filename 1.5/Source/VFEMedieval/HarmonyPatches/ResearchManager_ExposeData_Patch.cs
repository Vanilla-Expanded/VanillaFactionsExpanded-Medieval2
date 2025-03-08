using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "ExposeData")]
    public static class ResearchManager_ExposeData_Patch
    {
        public static Dictionary<ResearchProjectDef, float> finishedCosts = new Dictionary<ResearchProjectDef, float>();
        public static void Postfix()
        {
            Scribe_Collections.Look(ref finishedCosts, "finishedCosts", LookMode.Def, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                finishedCosts ??= new Dictionary<ResearchProjectDef, float>();
            }
        }
    }
}