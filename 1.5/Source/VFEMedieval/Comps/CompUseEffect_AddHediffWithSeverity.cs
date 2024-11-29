
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_AddHediffWithSeverity : CompUseEffect
    {
        public CompProperties_UseEffectAddHediffWithSeverity Props => (CompProperties_UseEffectAddHediffWithSeverity)props;

        public override void DoEffect(Pawn user)
        {
            Hediff hediff = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            float adjustedSeverity = (1 - user.GetStatValue(StatDefOf.ToxicResistance, true))*Props.severity;

            if (adjustedSeverity > 0)
            {
                if (hediff == null)
                {
                    user.health.AddHediff(Props.hediffDef);
                    hediff = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                    hediff.Severity = adjustedSeverity;
                }
                else
                {
                    hediff.Severity += adjustedSeverity;
                }
            }
            
        }

     
    }
}
