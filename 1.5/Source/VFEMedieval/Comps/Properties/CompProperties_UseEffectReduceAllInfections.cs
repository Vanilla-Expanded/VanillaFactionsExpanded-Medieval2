using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectReduceAllInfections : CompProperties_UseEffect
    {
        public float amount = 1;


        public CompProperties_UseEffectReduceAllInfections()
        {
            compClass = typeof(CompUseEffect_ReduceAllInfections);
        }
    }
}
