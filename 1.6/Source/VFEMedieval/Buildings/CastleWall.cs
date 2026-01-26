using Verse;
using RimWorld;

namespace VFEMedieval
{
    public class CastleWall : Building
    {
        public override int HitPoints
        {
            set
            {
                base.HitPoints = value;
                Notify_HitPointsChanged(value);
            }
        }

        public void Notify_HitPointsChanged(int hitPoints)
        {
            if (hitPoints <= 0 || hitPoints >= 0.3 * MaxHitPoints || this.Map == null)
                return;

            // Need to check before spawning a replacement, as it'll destroy (and deselect) the current wall
            var selected = Find.Selector.IsSelected(this);

            Thing thingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(VFEM_DefOf.VFEM2_DamagedCastleWall, this.Stuff), this.PositionHeld, this.Map);
            thingToMake.SetFaction(this.Faction);

            DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, thingToMake.HitPoints*0.5f);
            thingToMake.TakeDamage(dinfo);

            if (selected)
            {
                Find.Selector.Select(thingToMake, false, false);
            }

            if (this.Spawned)
            {
                this.Destroy();
            }
        }
    }
}
