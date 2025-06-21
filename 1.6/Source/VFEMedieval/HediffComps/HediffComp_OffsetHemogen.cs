
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_OffsetHemogen : HediffComp
    {


        public HediffCompProperties_OffsetHemogen Props => (HediffCompProperties_OffsetHemogen)props;


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            
           

            if (this.parent.pawn.IsHashIntervalTick(100, delta))
            {
                Pawn pawn = this.parent.pawn;

                float offset = this.parent.Severity > 0.65f ? Props.offsetOnPotent : Props.offsetOnFading;

                GeneUtility.OffsetHemogen(pawn, offset/600);
            }

           
        }

    }
}