using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectIncreasePsypower : CompProperties_UseEffect
    {
        public float amount = 1;
      

        public CompProperties_UseEffectIncreasePsypower()
        {
            compClass = typeof(CompUseEffect_IncreasePsypower);
        }
    }
}
