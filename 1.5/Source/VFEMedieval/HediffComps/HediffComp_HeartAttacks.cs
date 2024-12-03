using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_HeartAttacks : HediffComp
    {


        public HediffCompProperties_HeartAttacks Props => (HediffCompProperties_HeartAttacks)props;

        public static  List<BodyPartDef> bodyParts = new List<BodyPartDef>() { BodyPartDefOf.Heart};

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.IsHashIntervalTick(30000))
            {
              
                if (this.parent.Severity > 0.65f)
                {
                    if (Rand.Chance(0.02856f))
                    {
                        HediffGiverUtility.TryApply(pawn, VFEM_DefOf.HeartAttack, bodyParts);

                    }
                }
              
              
            }
        }

    }
}