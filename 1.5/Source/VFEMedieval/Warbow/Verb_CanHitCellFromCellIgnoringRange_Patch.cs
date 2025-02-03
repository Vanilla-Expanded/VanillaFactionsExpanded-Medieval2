using HarmonyLib;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Verb), "CanHitCellFromCellIgnoringRange")]
    public static class Verb_CanHitCellFromCellIgnoringRange_Patch
    {
        public static bool Prefix(Verb __instance, IntVec3 sourceSq, IntVec3 targetLoc, ref bool __result)
        {
            if (__instance.verbProps is VerbProperties_Warbow)
            {
                __result = targetLoc.Roofed(__instance.caster.Map) is false || GenSight.LineOfSight(sourceSq, targetLoc, __instance.caster.Map, skipFirstCell: true);
                return false;
            }
            return true;
        }
    }
}
