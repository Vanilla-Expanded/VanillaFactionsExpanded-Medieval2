using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "ApplyTechprint")]
    public static class ResearchManager_ApplyTechprint_Patch
    {
        public static void Prefix(ResearchProjectDef proj, out float __state)
        {
            ResearchManager_FinishProject_Patch.Prefix(proj, out __state);
        }

        public static void Postfix(ResearchProjectDef proj, float __state)
        {
            ResearchManager_FinishProject_Patch.Postfix(proj, __state);
        }
    }
}