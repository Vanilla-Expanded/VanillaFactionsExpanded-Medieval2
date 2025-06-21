using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public class CastleGate : Building_MultiTileDoor
    {
        protected int interval;

        public override void Tick()
        {
            base.Tick();
            if (openInt is false && ticksSinceOpen > 0)
            {
                ticksSinceOpen -= 9;
                if (ticksSinceOpen < 0)
                    ticksSinceOpen = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref interval, "interval");
        }
    }
}
