using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchProjectDef), "Cost", MethodType.Getter)]
    public static class ResearchProjectDef_Cost_Patch
    {
        public static bool allowCheckingCompletedResearch = false;
        public static void Prefix(ResearchProjectDef __instance)
        {
            ResearchUtility.TryChangeBaseCost(__instance, allowCheckingCompletedResearch: allowCheckingCompletedResearch);
        }
    }
}