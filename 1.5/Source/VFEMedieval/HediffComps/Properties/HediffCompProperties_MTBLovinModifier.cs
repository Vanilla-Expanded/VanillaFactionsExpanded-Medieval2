
using Verse;
namespace VFEMedieval
{
    public class HediffCompProperties_MTBLovinModifier : HediffCompProperties
    {

        public float MTBLovingModifierOnPotent = 0.75f;
        public float MTBLovingModifierOnFading = 1f;


        public HediffCompProperties_MTBLovinModifier()
        {
            compClass = typeof(HediffComp_MTBLovinModifier);
        }
    }
}
