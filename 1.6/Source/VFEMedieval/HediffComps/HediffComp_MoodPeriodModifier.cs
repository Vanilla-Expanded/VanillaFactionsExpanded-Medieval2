
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_MoodPeriodModifier : HediffComp
    {


        public HediffCompProperties_MoodPeriodModifier Props => (HediffCompProperties_MoodPeriodModifier)props;


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.IsHashIntervalTick(200, delta))
            {
               
                if (this.parent.Severity > 0.65f)
                {
                    StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn, Props.moodModifierOnPotent);
                }
                else
                {
                    StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn, Props.moodModifierOnFading);
                }
               
            }
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn,Props.moodModifierOnPotent);

        }

        public override void CompPostPostRemoved()
        {
            StaticCollections.RemovePawnMoodTimeMultiplierFromList(this.parent.pawn);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            StaticCollections.RemovePawnMoodTimeMultiplierFromList(this.parent.pawn);

        }

        public override void Notify_PawnKilled()
        {
            StaticCollections.RemovePawnMoodTimeMultiplierFromList(this.parent.pawn);

        }


    }
}