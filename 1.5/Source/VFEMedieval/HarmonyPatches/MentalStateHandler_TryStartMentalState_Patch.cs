using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public static class MentalStateHandler_TryStartMentalState_Patch
    {
        public static bool Prefix(Pawn ___pawn, MentalStateDef stateDef)
        {
            foreach (Hediff hediff in ___pawn.health.hediffSet.hediffs)
            {
                if (hediff.TryGetComp<HediffComp_MentalStatePrevention>() is HediffComp_MentalStatePrevention comp)
                {
                    if (comp.Props.preventMentalStates.Contains(stateDef))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}