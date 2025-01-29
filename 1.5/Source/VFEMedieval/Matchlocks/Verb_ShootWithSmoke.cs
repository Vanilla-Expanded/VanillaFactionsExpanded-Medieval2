using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class Verb_ShootWithSmoke : Verb_Shoot
    {
        public override bool TryCastShot()
        {
            var shotCount = (this.GetProjectile().projectile as ProjectilePropertiesMatchLock).shotCount;
            var result = base.TryCastShot();
            for (int i = 0; i < shotCount; i++)
            {
                if (base.TryCastShot())
                {
                    result = true;
                }
            }
            if (result)
            {
                Vector3 loc = this.caster.PositionHeld.ToVector3();
                Map mapHeld = this.caster.MapHeld;
                ThingDef projectile = this.GetProjectile();
                int? num;
                if (projectile == null)
                {
                    num = null;
                }
                else
                {
                    ProjectileProperties projectile2 = projectile.projectile;
                    num = ((projectile2 != null) ? new int?(projectile2.GetDamageAmount(this.caster, null)) : null);
                }
                int? num2 = num;
                float size = Mathf.Clamp01(((num2 != null) ? new float?((float)num2.GetValueOrDefault() / MEHNI_DECIDED_ON_MAGIC_NUMBER) : null) ?? 1f);
                ThrowFlintLockSmoke(loc, mapHeld, size);
                ThrowFlintLockSmoke(loc, mapHeld, size);
            }
            return result;
        }

        public static void ThrowFlintLockSmoke(Vector3 loc, Map map, float size)
        {
            if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(VFEM_DefOf.Grimworld_FlintlockSmoke, null);
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
            GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
        }

        private const float MEHNI_DECIDED_ON_MAGIC_NUMBER = 32f;
    }
}