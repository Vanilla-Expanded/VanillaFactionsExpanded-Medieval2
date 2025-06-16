using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(MentalBreakWorker), "TryStart")]
    public static class MentalBreakWorker_TryStart_Patch
    {
        public static bool Prefix(Pawn pawn, MentalBreakWorker __instance)
        {
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.TryGetComp<HediffComp_MentalStatePrevention>() is HediffComp_MentalStatePrevention comp)
                {
                    if (comp.Props.preventMentalStates.Contains(__instance.def.mentalState))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}