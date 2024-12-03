
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_ResistancePerSeverity : HediffComp
    {


        public HediffCompProperties_ResistancePerSeverity Props => (HediffCompProperties_ResistancePerSeverity)props;



        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.IsHashIntervalTick(3600))
            {
                float loss;
                if (this.parent.Severity > 0.65f)
                {
                    loss = Props.resistanceLossOnPotent;
                }
                else
                {
                    loss = Props.resistanceLossOnFading;
                }
                if (pawn.guest != null && pawn.guest.resistance > 0f)
                {
                    pawn.guest.resistance = Mathf.Max(0f, pawn.guest.resistance - loss);
                }
            }
        }

    }
}