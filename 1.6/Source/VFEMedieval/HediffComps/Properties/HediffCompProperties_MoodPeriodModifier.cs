
using Verse;
namespace VFEMedieval
{
    public class HediffCompProperties_MoodPeriodModifier : HediffCompProperties
    {

        public float moodModifierOnPotent = 0.25f;
        public float moodModifierOnFading = 0.5f;


        public HediffCompProperties_MoodPeriodModifier()
        {
            compClass = typeof(HediffComp_MoodPeriodModifier);
        }
    }
}
