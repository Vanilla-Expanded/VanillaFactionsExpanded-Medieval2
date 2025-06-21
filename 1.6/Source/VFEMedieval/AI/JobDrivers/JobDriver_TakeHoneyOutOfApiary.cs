using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace VFEMedieval
{
    class JobDriver_TakeHoneyOutOfApiary : JobDriver
    {
       

        protected Building_Apiary Apiary
        {
            get
            {
                return (Building_Apiary)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Thing Honey
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Apiary;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Apiary.HoneyReady).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            var toil = ToilMaker.MakeToil();
            toil.initAction = () =>
            {
                Thing thing = this.Apiary.TakeOutHoney();
                GenPlace.TryPlaceThing(thing, this.pawn.Position, base.Map, ThingPlaceMode.Near, null, null);
                StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, base.Map, currentPriority, this.pawn.Faction, out var c, true))
                {
                    this.job.SetTarget(TargetIndex.C, c);
                    this.job.SetTarget(TargetIndex.B, thing);
                    this.job.count = thing.stackCount;
                }
                else
                {
                    base.EndJobWith(JobCondition.Incompletable);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
        }
    }
}
