using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class ProjectilePropertiesMatchLock : ProjectileProperties
    {

    }

    [HarmonyPatch(typeof(ProjectileProperties), "GetArmorPenetration", new Type[] { typeof(float), typeof(StringBuilder) })]
    public static class ProjectileProperties_GetArmorPenetration_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var codes = instructions.ToList();
            bool found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (i > 2 && codes[i - 2].opcode == OpCodes.Ldloc_0
                    && codes[i - 1].opcode == OpCodes.Ldc_R4 && codes[i - 1].operand is float floatOperand 
                    && floatOperand == 0.0f && codes[i].opcode == OpCodes.Bge_Un_S && codes[i].operand is Label label)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(ProjectilePropertiesMatchLock));
                    yield return new CodeInstruction(OpCodes.Brtrue, label);
                    found = true;
                }
            }

            if (found is false)
            {
                Log.Error("VFEMedieval: Transpiler for ProjectileProperties.GetArmorPenetration_Patch failed to find pattern. Patch might not work correctly.");
            }
        }
    }

    [HotSwappable]
    public class MatchlockProjectile : Bullet
    {
        private Thing equipment;
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            this.equipment = equipment;
        }

        public override int DamageAmount
        {
            get
            {
                float maxRange = GetMaxRange();
                float distance = this.Position.DistanceTo(this.origin.ToIntVec3());
                float damageMultiplier = Mathf.Clamp01(1.0f - (distance / maxRange)) * 2.0f;
                var newDamage = Mathf.RoundToInt(base.DamageAmount * damageMultiplier);
                return newDamage;
            }
        }

        public override float ArmorPenetration
        {
            get
            {
                var ap = base.ArmorPenetration;
                Log.Message("AP: " + ap);
                return ap;
            }
        }

        private float GetMaxRange()
        {
            var comp = equipment.TryGetComp<CompEquippable>();
            if (comp != null)
            {
                return comp.PrimaryVerb.verbProps.range;
            }
            throw new Exception("[VFEMedieval] Couldn't determine max range for " + this.Label);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref equipment, "equipment");
        }
    }
}