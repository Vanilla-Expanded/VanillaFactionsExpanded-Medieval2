using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public class MatchlockProjectile : Bullet
    {
        private Thing equipment;
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            this.equipment = equipment;
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            curProjectile = this;
        }

        public static bool isImpacting;

        public static MatchlockProjectile curProjectile;

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            isImpacting = true;
            try
            {
                base.Impact(hitThing, blockedByShield);
            }
            finally
            {
                isImpacting = false;
            }
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

        private float GetMaxRange()
        {
            var comp = equipment.TryGetComp<CompEquippable>();
            if (comp != null)
            {
                return comp.PrimaryVerb.verbProps.range;
            }
            else if (equipment is Building_TurretGun building_TurretGun)
            {
                return building_TurretGun.AttackVerb.verbProps.range;
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
