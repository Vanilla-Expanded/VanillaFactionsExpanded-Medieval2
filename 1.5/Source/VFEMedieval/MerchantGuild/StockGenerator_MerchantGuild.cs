using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public class StockGenerator_MerchantGuild : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (HandlesThingDef(def))
                {
                    foreach (Thing item in StockGeneratorUtility.TryMakeForStock(def, Rand.RangeInclusive(1, 3), faction))
                    {
                        yield return item;
                    }
                }
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            if (thingDef != null && thingDef.tradeability.TraderCanSell() && thingDef.category == ThingCategory.Item)
            {
                if (typeof(GeneSetHolderBase).IsAssignableFrom(thingDef.thingClass) || thingDef.BaseMarketValue <= 0)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}