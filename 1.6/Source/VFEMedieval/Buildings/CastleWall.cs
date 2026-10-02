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
            // Integer comparison: 30% of MaxHitPoints is not always exactly representable as a
            // float, so "hitPoints >= 0.3 * MaxHitPoints" could read a wall exactly at the
            // boundary as still below it.
            if (hitPoints <= 0 || 10 * hitPoints >= 3 * MaxHitPoints || this.Map == null)
                return;

            // Need to check before spawning a replacement, as it'll destroy (and deselect) the current wall
            var selected = Find.Selector.IsSelected(this);

            Thing thingToMake = ThingMaker.MakeThing(VFEM_DefOf.VFEM2_DamagedCastleWall, this.Stuff);
            // Set health before spawning, while Map is still null: the setter above re-enters
            // Notify_HitPointsChanged, whose own guard returns immediately without a Map, so this
            // cannot trigger a second, unwanted swap once the replacement is actually spawned.
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
