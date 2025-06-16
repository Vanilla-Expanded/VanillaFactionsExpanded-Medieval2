using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectAddInspiration : CompProperties_UseEffect
    {
        public float chance = 1;


        public CompProperties_UseEffectAddInspiration()
        {
            compClass = typeof(CompUseEffect_AddInspiration);
        }
    }
}
