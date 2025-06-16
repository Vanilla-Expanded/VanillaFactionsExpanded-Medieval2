
using Verse;
namespace VFEMedieval
{
    public class HediffCompProperties_ResistancePerSeverity : HediffCompProperties
    {

        public float resistanceLossOnPotent = 0.25f;
        public float resistanceLossOnFading = 0.08f;


        public HediffCompProperties_ResistancePerSeverity()
        {
            compClass = typeof(HediffComp_ResistancePerSeverity);
        }
    }
}
