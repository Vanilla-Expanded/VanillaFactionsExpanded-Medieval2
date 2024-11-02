using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    class JobDriver_TrainAtDummy : JobDriver
    {
        public int initialXP = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialXP, "initialXP", 0);
        }

        public override object[] TaleParameters()
        {
            return new object[2]
            {
                pawn,
                TargetA.Thing.def
            };
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                initialXP = TargetThingA.HitPoints;
            };
            toil.tickAction = delegate ()
            {
                pawn.rotationTracker.FaceTarget(TargetA);
                pawn.GainComfortFromCellIfPossible(false);
                if (pawn.meleeVerbs.TryMeleeAttack(TargetA.Thing))
                {
                    pawn.skills.Learn(SkillDefOf.Melee, 30f, false);
                    TargetThingA.HitPoints = initialXP;
                }
                Building joySource = (Building)TargetThingA;
                JoyUtility.JoyTickCheckEnd(pawn, job.doUntilGatheringEnded ? JoyTickFullJoyAction.None : JoyTickFullJoyAction.EndJob, 1f, joySource);
            };
            toil.handlingFacing = true;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = job.doUntilGatheringEnded ? job.expiryInterval : job.def.joyDuration;
            toil.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            yield return toil;
            yield break;
        }
    }
}