using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;

namespace VFEMedieval
{
    [HotSwappable]
    public class StockGenerator_MerchantGuild : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            foreach (var traderKind in DefDatabase<TraderKindDef>.AllDefs.Where(x => x != VFEM_DefOf.VFEM2_MerchantGuildTrader))
            {
                foreach (var generator in traderKind.stockGenerators)
                {
                    foreach (Thing item in GetThingsFrom(generator, forTile, faction))
                    {
                        yield return item;
                    }
                }
            }
        }

        public static List<Thing> GetThingsFrom(StockGenerator generator, int forTile, Faction faction = null)
        {
            List<Thing> things = new List<Thing>();
            try
            {
                foreach (Thing item in generator.GenerateThings(forTile, faction))
                {
                    things.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Got exception from generating things from " + generator + " - " + ex);
            }
            return things;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return true;
        }
    }
}