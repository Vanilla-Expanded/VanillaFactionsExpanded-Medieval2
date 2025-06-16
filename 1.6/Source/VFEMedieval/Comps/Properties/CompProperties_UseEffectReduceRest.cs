using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectReduceRest : CompProperties_UseEffect
    {
        public float amount = 1;
      

        public CompProperties_UseEffectReduceRest()
        {
            compClass = typeof(CompUseEffect_ReduceRest);
        }
    }
}
