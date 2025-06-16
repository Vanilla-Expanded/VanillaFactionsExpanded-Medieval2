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
                var casterMap = __instance.caster.Map;
                var casterRoofed = sourceSq.Roofed(casterMap);
                var targetRoofed = targetLoc.Roofed(casterMap);

                // If the target is in LoS, always allow the shot.
                if (GenSight.LineOfSight(sourceSq, targetLoc, casterMap, skipFirstCell: true))
                {
                    __result = true;
                    return false;
                }

                // If both the caster and target are unroofed, allow the shot.
                if (!casterRoofed && !targetRoofed)
                {
                    __result = true;
                    return false;
                }

                // If the caster is under a roof, they need LoS to fire.
                if (casterRoofed)
                {
                    __result = false;
                    return false;
                }

                // If the target is under a roof while the caster is not, they cannot be hit.
                if (!casterRoofed && targetRoofed)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
