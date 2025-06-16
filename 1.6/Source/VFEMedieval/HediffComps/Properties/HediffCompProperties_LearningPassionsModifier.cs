
using Verse;
namespace VFEMedieval
{
    public class HediffCompProperties_LearningPassionsModifier : HediffCompProperties
    {

        public float learningModifierSinglePassionOnPotent = 0.25f;
        public float learningModifierDoublePassionOnPotent = 0.5f;

        public float learningModifierSinglePassionOnFading = 0.1f;
        public float learningModifierDoublePassionOnFading = 0.2f;



        public HediffCompProperties_LearningPassionsModifier()
        {
            compClass = typeof(HediffComp_LearningPassionsModifier);
        }
    }
}
