using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using VFEMedieval;
using System.Linq;

namespace VFEMedieval
{
    public class Autofilling_Shelf : Building_Storage
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                RandomizeContents();
            }
        }

        public void RandomizeContents()
        {
           
            List<StockGenerator> listTribal = DefDatabase<TraderKindDef>.GetNamedSilentFail("Base_Neolithic_Standard")?.stockGenerators.Where(sg => !(sg is StockGenerator_BuyExpensiveSimple)&&!(sg is StockGenerator_BuyTradeTag)).ToList();
            if (!listTribal.NullOrEmpty())
            {
                int numberOfItems = new IntRange(1, 5).RandomInRange;

                for (int i = 0; i < numberOfItems; i++)
                {
                    StockGenerator gen = listTribal.RandomElement();
                    List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefs.Where(d => gen?.HandlesThingDef(d) == true && !d.IsEdifice() && !d.IsCorpse && d.race==null && d.techLevel<TechLevel.Medieval && d.tradeTags?.Contains("Serum")!=true && !d.defName.Contains("ABooks_")).ToList();
                    if (!thingDefs.NullOrEmpty())
                    {
                        ThingDef thingToAdd = thingDefs.RandomElement();
                        if (thingToAdd != null)
                        {
                            Thing thing = ThingMaker.MakeThing(thingToAdd, thingToAdd.MadeFromStuff ? GenStuff.DefaultStuffFor(thingToAdd) : null);

                            if (thingToAdd.ingestible!=null || thingToAdd.thingCategories?.Contains(VFEM_DefOf.ResourcesRaw) == true)
                            {
                                thing.stackCount = new IntRange(1, 30).RandomInRange;
                            }
                            GenPlace.TryPlaceThing(thing, this.OccupiedRect().RandomCell, this.Map, ThingPlaceMode.Direct);
                        }
                    }
                    
                   
                }
            }
            

        }

    }
}
