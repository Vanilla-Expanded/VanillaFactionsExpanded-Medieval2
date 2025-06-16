using HarmonyLib;
using RimWorld;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchManager), "ResearchPerformed")]
    public static class ResearchManager_ResearchPerformed_Patch
    {
        public static void Prefix(ref float amount)
        {
            amount /= ResearchPatchesHelper.GetMaynardCostFactor();
        }
    }
}