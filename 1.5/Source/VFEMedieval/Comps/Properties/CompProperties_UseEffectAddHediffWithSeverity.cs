using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectAddHediffWithSeverity : CompProperties_UseEffect
    {
        public float severity = 1;
        public HediffDef hediffDef;

        public CompProperties_UseEffectAddHediffWithSeverity()
        {
            compClass = typeof(CompUseEffect_AddHediffWithSeverity);
        }
    }
}
