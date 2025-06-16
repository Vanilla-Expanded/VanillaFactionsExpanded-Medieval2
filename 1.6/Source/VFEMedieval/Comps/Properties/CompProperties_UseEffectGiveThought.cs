using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_UseEffectGiveThought : CompProperties_UseEffect
    {
        public ThoughtDef thought;


        public CompProperties_UseEffectGiveThought()
        {
            compClass = typeof(CompUseEffect_GiveThought);
        }
    }
}
