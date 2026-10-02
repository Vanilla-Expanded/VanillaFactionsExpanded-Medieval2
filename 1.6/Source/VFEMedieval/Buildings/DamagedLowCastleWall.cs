using Verse;
using RimWorld;

namespace VFEMedieval
{
    public class DamagedLowCastleWall : Building
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
            if (hitPoints <= 0 || 10 * hitPoints <= 3 * MaxHitPoints || this.Map == null)
                return;

            // Need to check before spawning a replacement, as it'll destroy (and deselect) the current wall
            var selected = Find.Selector.IsSelected(this);

            Thing thingToMake = ThingMaker.MakeThing(VFEM_DefOf.VFEM2_LowCastleWall, this.Stuff);
            thingToMake.HitPoints = System.Math.Max(1, System.Math.Min(hitPoints, thingToMake.MaxHitPoints));
            thingToMake.SetFaction(this.Faction);
            GenSpawn.Spawn(thingToMake, this.PositionHeld, this.Map, this.Rotation);

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
