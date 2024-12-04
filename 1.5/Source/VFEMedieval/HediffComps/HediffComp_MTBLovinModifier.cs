
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_MTBLovinModifier : HediffComp
    {


        public HediffCompProperties_MTBLovinModifier Props => (HediffCompProperties_MTBLovinModifier)props;


        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.IsHashIntervalTick(200))
            {

                if (this.parent.Severity > 0.65f)
                {
                    StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn, Props.MTBLovingModifierOnPotent);
                }
                else
                {
                    StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn, Props.MTBLovingModifierOnFading);
                }

            }
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            StaticCollections.AddPawnMoodTimeMultiplierToList(this.parent.pawn, Props.MTBLovingModifierOnPotent);

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