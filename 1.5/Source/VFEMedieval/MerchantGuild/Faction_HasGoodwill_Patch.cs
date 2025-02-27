using HarmonyLib;
using RimWorld;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Faction), "HasGoodwill", MethodType.Getter)]
    public static class Faction_HasGoodwill_Patch
    {
        public static void Postfix(Faction __instance, ref bool __result)
        {
            if (__result is false && __instance?.def == VFEM_DefOf.VFEM2_MerchantGuild)
            {
                __result = true;
            }
        }
    }
}