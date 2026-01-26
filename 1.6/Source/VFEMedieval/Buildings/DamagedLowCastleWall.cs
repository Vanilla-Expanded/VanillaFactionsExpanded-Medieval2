using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using VFEMedieval;

namespace VFEMedieval
{
    public class DamagedLowCastleWall : Building
    {

        public override void TickRare()
        {
            base.TickRare();

            if (this.HitPoints > 0.3 * MaxHitPoints)
            {
                // Need to check before spawning a replacement, as it'll destroy (and deselect) the current wall
                var selected = Find.Selector.IsSelected(this);

                Thing thingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(VFEM_DefOf.VFEM2_LowCastleWall, this.Stuff), this.PositionHeld, this.Map);
                thingToMake.SetFaction(this.Faction);
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, thingToMake.HitPoints * 0.4f);
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
}
