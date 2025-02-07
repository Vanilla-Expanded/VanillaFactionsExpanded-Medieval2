
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_AddHediffWithSeverity : CompUseEffect
    {
        public CompProperties_UseEffectAddHediffWithSeverity Props => (CompProperties_UseEffectAddHediffWithSeverity)props;

        public override void DoEffect(Pawn user)
        {
            if(Props.neededGene==null || (Props.neededGene!=null&& user.genes?.HasActiveGene(Props.neededGene) == true))
            {
                Hediff hediff = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                float adjustedSeverity = Props.scaleSeverityByToxResistance ?  (1 - user.GetStatValue(StatDefOf.ToxicResistance, true)) * Props.severity : Props.severity;

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
                else
                {
                    if(hediff!=null)
                    {
                        hediff.Severity += adjustedSeverity;
                    }
                }
            }
            
            
        }

     
    }
}
