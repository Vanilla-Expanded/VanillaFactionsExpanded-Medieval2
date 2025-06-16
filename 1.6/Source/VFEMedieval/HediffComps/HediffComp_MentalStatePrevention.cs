using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_MentalStatePrevention : HediffComp
    {
        public HediffCompProperties_MentalStatePrevention Props => (HediffCompProperties_MentalStatePrevention)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Pawn.MentalState != null)
            {
                Pawn.MentalState.RecoverFromState();
            }
        }
    }

    public class HediffCompProperties_MentalStatePrevention : HediffCompProperties
    {
        public List<MentalStateDef> preventMentalStates = new List<MentalStateDef>();

        public HediffCompProperties_MentalStatePrevention()
        {
            compClass = typeof(HediffComp_MentalStatePrevention);
        }
    }
}