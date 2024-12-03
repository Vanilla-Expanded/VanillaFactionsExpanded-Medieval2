
using Verse;
namespace VFEMedieval
{
    public class HediffCompProperties_OffsetHemogen : HediffCompProperties
    {

        public float offsetOnPotent = 0.08f;
        public float offsetOnFading = 0.02f;


        public HediffCompProperties_OffsetHemogen()
        {
            compClass = typeof(HediffComp_OffsetHemogen);
        }
    }
}
