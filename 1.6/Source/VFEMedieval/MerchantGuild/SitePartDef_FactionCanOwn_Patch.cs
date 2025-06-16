using HarmonyLib;
using RimWorld;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(SitePartDef), "FactionCanOwn")]
    public static class SitePartDef_FactionCanOwn_Patch
    {
        public static void Postfix(SitePartDef __instance, Faction faction, ref bool __result)
        {
            if (__result && faction != null && faction.def == VFEM_DefOf.VFEM2_MerchantGuild)
            {
                __result = false;
            }
        }
    }
}