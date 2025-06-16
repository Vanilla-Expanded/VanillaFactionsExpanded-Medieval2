using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappableAttribute]
    public class JobDriver_PlayArchery : JobDriver_WatchBuilding
    {
        Thing bow;
        public override void WatchTickAction(int delta)
        {
            if (pawn.IsHashIntervalTick(400,delta))
            {
                ThrowObjectAt(pawn, base.TargetA.Cell, VFEM_DefOf.VFEM2_ArrowThrowable);
            }
            base.WatchTickAction(delta);
        }

        public delegate bool EverPossibleToWatchFrom(IntVec3 watchCell, IntVec3 buildingCenter, Map map, bool bedAllowed, ThingDef def);
        public static readonly EverPossibleToWatchFrom everPossibleToWatchFrom = AccessTools.MethodDelegate<EverPossibleToWatchFrom>(AccessTools.Method(typeof(WatchBuildingUtility), "EverPossibleToWatchFrom"));
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => everPossibleToWatchFrom(this.TargetB.Cell, this.TargetA.Cell, pawn.Map, false, TargetA.Thing.def) is false);
            return base.MakeNewToils();
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
            if (bow is null)
            {
                bow = ThingMaker.MakeThing(VFEM_DefOf.Bow_Great, GenStuff.DefaultStuffFor(VFEM_DefOf.Bow_Great));
            }
            DrawEquipmentAiming(bow, drawLoc, num);
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
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
        }
        private static void ThrowObjectAt(Pawn thrower, IntVec3 targetCell, FleckDef fleck)
        {
            var targetPos = targetCell.ToVector3Shifted().Yto0();
            float num = Rand.Range(8f, 10f);
            Vector3 vector = targetPos +
                RandomHorizontalOffset(thrower, targetCell,
                (1f - (float)thrower.skills.GetSkill(SkillDefOf.Shooting).Level / 20f));
            vector.y = thrower.DrawPos.y;
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(thrower.DrawPos, thrower.Map, fleck);
            dataStatic.rotationRate = 0;
            dataStatic.rotation = thrower.Rotation.AsAngle;
            dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
            dataStatic.velocitySpeed = num;
            dataStatic.airTimeLeft = Mathf.RoundToInt((dataStatic.spawnPosition - vector).MagnitudeHorizontal() / (num));
            thrower.Map.flecks.CreateFleck(dataStatic);
        }

        public static Vector3 RandomHorizontalOffset(Pawn caster, IntVec3 targetCell, float maxDist)
        {
            float num = Rand.Range(0f, maxDist);
            var angle = (caster.TrueCenter().Yto0() - targetCell.ToVector3Shifted().Yto0()).AngleFlat();
            float y = Rand.Range(angle - 115, angle + 115);
            return Quaternion.Euler(new Vector3(0f, y, 0f)) * Vector3.forward * num;
        }
    }
}