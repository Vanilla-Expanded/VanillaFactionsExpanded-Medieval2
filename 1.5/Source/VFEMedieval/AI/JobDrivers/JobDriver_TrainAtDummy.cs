using System.Collections.Generic;
using RimWorld;
using UnityEngine.UIElements;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    class JobDriver_TrainAtDummy : JobDriver
    {
        public int initialXP = 0;
        Thing weapon;
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


        public void DrawEquipment(Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            if (pawn.pather.Moving)
            {
                return;
            }
            Vector3 drawLoc = new Vector3(0f, (pawnRotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f, 0f);
            Vector3 vector = TargetA.Thing.DrawPos;
            float num = 0f;
            if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
            {
                num = (vector - pawn.DrawPos).AngleFlat();
            }
            drawLoc += rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
            if (weapon is null)
            {
                weapon = pawn.equipment.Primary;              
            }
            if (weapon != null)
            {
                DrawEquipmentAiming(weapon, drawLoc, num);
            }
           
        }

        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Mesh mesh = null;
            float num = aimAngle - 90f;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
            if (compEquippable != null)
            {
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                drawLoc += drawOffset;
                num += angleOffset;
            }
            Material material = null;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
        }


    }
}