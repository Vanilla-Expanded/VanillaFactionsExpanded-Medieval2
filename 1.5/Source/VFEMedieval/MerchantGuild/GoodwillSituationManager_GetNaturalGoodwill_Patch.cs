using HarmonyLib;
using RimWorld;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(GoodwillSituationManager), "GetNaturalGoodwill")]
    public static class GoodwillSituationManager_GetNaturalGoodwill_Patch
    {
        public static void Postfix(ref int __result, Faction other)
        {
            if (other != null && other?.def == VFEM_DefOf.VFEM2_MerchantGuild)
            {
                __result = 0;
            }
        }
    }
}