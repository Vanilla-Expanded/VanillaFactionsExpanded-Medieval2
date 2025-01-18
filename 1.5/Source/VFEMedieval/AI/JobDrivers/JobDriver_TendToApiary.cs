using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using VFEMedieval;

namespace VFEMedieval
{
    class JobDriver_TendToApiary : JobDriver
    {
        protected Building_Apiary Apiary
        {
            get
            {
                return (Building_Apiary)this.job.GetTarget(TargetIndex.A).Thing;
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
            yield return Toils_General.Wait(500, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Apiary.needTend).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    int skill = pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").levelInt / 2;
                    if (Rand.RangeInclusive(0, 11 - skill) <= 5)
                    {
                        this.Apiary.tickBeforeTend += 120000;
                    }
                    else
                    {
                       
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(VFEM_DefOf.VFEM2_StingMoodDebuff);
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Tending failed", 5f);
                        pawn.jobs.StartJob(new Job(VFEM_DefOf.VFEM2_TendToApiary, TargetA));
                    }
                    pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").Learn(20, false);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
