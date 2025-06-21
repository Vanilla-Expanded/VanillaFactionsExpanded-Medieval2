
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_LearningPassionsModifier : HediffComp
    {


        public HediffCompProperties_LearningPassionsModifier Props => (HediffCompProperties_LearningPassionsModifier)props;


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.IsHashIntervalTick(200, delta))
            {

                if (this.parent.Severity > 0.65f)
                {
                    StaticCollections.AddPawnLearningFactorSinglePassionMultiplierToList(this.parent.pawn, Props.learningModifierSinglePassionOnPotent);
                    StaticCollections.AddPawnLearningFactorDoublePassionMultiplierToList(this.parent.pawn, Props.learningModifierDoublePassionOnPotent);

                }
                else
                {
                    StaticCollections.AddPawnLearningFactorSinglePassionMultiplierToList(this.parent.pawn, Props.learningModifierSinglePassionOnFading);
                    StaticCollections.AddPawnLearningFactorDoublePassionMultiplierToList(this.parent.pawn, Props.learningModifierDoublePassionOnFading);

                }

            }
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            StaticCollections.AddPawnLearningFactorSinglePassionMultiplierToList(this.parent.pawn, Props.learningModifierSinglePassionOnPotent);
            StaticCollections.AddPawnLearningFactorDoublePassionMultiplierToList(this.parent.pawn, Props.learningModifierDoublePassionOnPotent);


        }

        public override void CompPostPostRemoved()
        {
            StaticCollections.RemovePawnLearningFactorSinglePassionMultiplierFromList(this.parent.pawn);
            StaticCollections.RemovePawnLearningFactorDoublePassionMultiplierFromList(this.parent.pawn);


        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollections.RemovePawnLearningFactorSinglePassionMultiplierFromList(this.parent.pawn);
            StaticCollections.RemovePawnLearningFactorDoublePassionMultiplierFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            StaticCollections.RemovePawnLearningFactorSinglePassionMultiplierFromList(this.parent.pawn);
            StaticCollections.RemovePawnLearningFactorDoublePassionMultiplierFromList(this.parent.pawn);

        }


    }
}