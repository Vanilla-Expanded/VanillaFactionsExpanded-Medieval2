using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchProjectDef), "Cost", MethodType.Getter)]
    public static class ResearchProjectDef_Cost_Patch
    {
        public static void Prefix(ResearchProjectDef __instance, out float __state)
        {
            ResearchManager_FinishProject_Patch.Prefix(__instance, out __state);
        }

        public static void Postfix(ResearchProjectDef __instance, float __state)
        {
            __instance.baseCost = __state;
        }
    }
}