using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_ReduceAllInfections : CompUseEffect
    {
        public CompProperties_UseEffectReduceAllInfections Props => (CompProperties_UseEffectReduceAllInfections)props;

        public override void DoEffect(Pawn user)
        {

            if (user.health != null && user.health.hediffSet.HasImmunizableNotImmuneHediff() && user.health.hediffSet.HasHediff(HediffDefOf.WoundInfection) && user.health.immunity != null && user.health.immunity.GetImmunity(HediffDefOf.WoundInfection) != 1)
            {

                List<Hediff> infectionHediffs = new List<Hediff>();
                user.health.hediffSet.GetHediffs(ref infectionHediffs, x => x.def == HediffDefOf.WoundInfection);
                if (infectionHediffs.Count > 0)
                {
                    foreach(Hediff infectionHediff in infectionHediffs)
                    {
                        if(infectionHediff.Severity> Props.amount)
                        {
                            infectionHediff.Severity -= Props.amount;
                        }else infectionHediff.Severity = 0;

                    }
                }
            }


        }


    }
}
