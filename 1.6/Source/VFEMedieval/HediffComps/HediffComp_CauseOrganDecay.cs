using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_CauseOrganDecay : HediffComp
    {


        public HediffCompProperties_CauseOrganDecay Props => (HediffCompProperties_CauseOrganDecay)props;

        public static List<BodyPartDef> bodyParts = new List<BodyPartDef>() { BodyPartDefOf.Heart, BodyPartDefOf.Lung, VFEM_DefOf.Kidney };

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            Pawn pawn = this.parent.pawn;

            if (this.parent.Severity > 0.9f)
            {
                if (pawn.IsHashIntervalTick(3600, delta))
                {
                    HediffGiverUtility.TryApply(pawn, HediffDefOf.OrganDecay, bodyParts);
                }
            }
            else
            {
                if (pawn.IsHashIntervalTick(10800, delta))
                {
                    HediffGiverUtility.TryApply(pawn, HediffDefOf.OrganDecay, bodyParts);
                }
            }

        }

    }
}