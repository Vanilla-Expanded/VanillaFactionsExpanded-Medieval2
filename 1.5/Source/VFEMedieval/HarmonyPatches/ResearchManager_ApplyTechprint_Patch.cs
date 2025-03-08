using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "ApplyTechprint")]
    public static class ResearchManager_ApplyTechprint_Patch
    {
        public static void Prefix(ResearchProjectDef proj)
        {
            ResearchUtility.TryChangeBaseCost(proj);
        }
    }
}