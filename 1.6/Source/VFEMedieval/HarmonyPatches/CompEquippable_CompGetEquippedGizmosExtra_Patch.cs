using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(CompEquippable), "CompGetEquippedGizmosExtra")]
    public static class CompEquippable_CompGetEquippedGizmosExtra_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompEquippable __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }
            if (__instance.parent.TryGetComp<CompRallyAbility>() is CompRallyAbility rallyComp)
            {
                yield return rallyComp.GetRallyGizmo();
            }
        }
    }
}