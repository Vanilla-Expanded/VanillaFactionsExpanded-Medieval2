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
            if (openInt is false)
            {
                for (var i = 0; i < 9; i++)
                {
                    if (ticksSinceOpen > 0)
                    {
                        ticksSinceOpen--;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref interval, "interval");
        }
    }
}
