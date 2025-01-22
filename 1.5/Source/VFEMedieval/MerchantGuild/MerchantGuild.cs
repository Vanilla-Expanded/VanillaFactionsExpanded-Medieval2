using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public class MerchantGuild : WorldObject, ILoadReferenceable, ITrader, IThingHolder
    {
        private Material cachedMat;

        public MerchantGuild_PathFollower pather;

        public MerchantGuild_Tweener tweener;

        public int TicksPerMove => 3300;

        public override Vector3 DrawPos => tweener.TweenedPos;

        private ThingOwner<Thing> stock;

        private int lastStockGenerationTicks = -1;

        private bool everGeneratedStock;

        private List<Pawn> tmpSavedPawns = new List<Pawn>();

        public MerchantGuild()
        {
            pather = new MerchantGuild_PathFollower(this);
            tweener = new MerchantGuild_Tweener(this);
        }

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(base.def.expandingIconTexture, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public override Texture2D ExpandingIcon => ContentFinder<Texture2D>.Get(base.def.texture);

        public TraderKindDef TraderKind => VFEM_DefOf.VFEM2_MerchantGuildTrader;

        public IEnumerable<Thing> Goods
        {
            get
            {
                if (stock == null)
                {
                    RegenerateStock();
                }
                return stock.InnerListForReading;
            }
        }

        public int RandomPriceFactorSeed => Gen.HashCombineInt(ID, 1048146365);

        public string TraderName => this.LabelCap;

        public bool CanTradeNow => true;

        public float TradePriceImprovementOffsetForPlayer => 0f;

        public TradeCurrency TradeCurrency => TradeCurrency.Silver;

        private int ticksToStay;

        public override void PostAdd()
        {
            base.PostAdd();
            TryStartPathing();
        }

        public override string GetInspectString()
        {
            if (pather.MovingNow is false)
            {
                return "VFEM2_MerchantCaravanStayPeriod".Translate(ticksToStay.ToStringTicksToPeriod());
            }
            return "";
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandShowSellableItems".Translate();
            command_Action.defaultDesc = "CommandShowSellableItemsDesc".Translate();
            command_Action.icon = Settlement.ShowSellableItemsCommand;
            command_Action.action = delegate
            {
                Find.WindowStack.Add(new Dialog_SellableItems(this));
                RoyalTitleDef titleRequiredToTrade = TraderKind.TitleRequiredToTrade;
                if (titleRequiredToTrade != null)
                {
                    TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradingRequiresPermit, titleRequiredToTrade.GetLabelCapForBothGenders());
                }
            };
            yield return command_Action;

            if (DebugSettings.ShowDevGizmos)
            {
                if (pather.MovingNow is false)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Start pathing",
                        action = delegate
                        {
                            TryStartPathing();
                        }
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Stop pathing",
                        action = delegate
                        {
                            pather.StopDead();
                        }
                    };
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
            {
                yield return floatMenuOption;
            }
            if (pather.MovingNow is false)
            {
                foreach (FloatMenuOption floatMenuOption3 in CaravanArrivalAction_Barter.GetFloatMenuOptions(caravan, this))
                {
                    yield return floatMenuOption3;
                }
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            tweener.ResetTweenedPosToRoot();
        }

        public override void Tick()
        {
            base.Tick();
            pather.PatherTick();
            tweener.TweenerTick();
            TraderTrackerTick();
            if (pather.MovingNow is false)
            {
                if (ticksToStay > 0)
                {
                    ticksToStay--;
                    if (ticksToStay <= 0)
                    {
                        TryStartPathing();
                    }
                }
                else
                {
                    ticksToStay = (int)(GenDate.TicksPerDay * Rand.Range(5f, 30f));
                }
            }
        }

        private void TryStartPathing()
        {
            TryDestroyStock();
            if (TileFinder.TryFindPassableTileWithTraversalDistance(Tile, 20, 30, out var result))
            {
                int num = BestGotoDestNear(result);
                if (num >= 0)
                {
                    this.pather.StartPath(num, repathImmediately: true);
                }
            }
        }

        public int BestGotoDestNear(int tile)
        {
            Predicate<int> predicate = delegate (int t)
            {
                if (Find.World.Impassable(t))
                {
                    return false;
                }
                return CanReach(tile) ? true : false;
            };
            if (predicate(tile))
            {
                return tile;
            }
            GenWorldClosest.TryFindClosestTile(tile, predicate, out var foundTile, 50);
            return foundTile;
        }

        public bool CanReach(int tile)
        {
            return Find.WorldReachability.CanReach(this.Tile, tile);
        }

        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            Caravan caravan = playerNegotiator.GetCaravan();
            foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
            {
                yield return item;
            }
            List<Pawn> pawns = caravan.PawnsListForReading;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (!caravan.IsOwner(pawns[i]))
                {
                    yield return pawns[i];
                }
            }
        }

        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (stock == null)
            {
                RegenerateStock();
            }
            Caravan caravan = playerNegotiator.GetCaravan();
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
            if (toGive is Pawn pawn)
            {
                CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
                if (!pawn.RaceProps.Humanlike && !stock.TryAdd(pawn, canMergeWithExistingStacks: false))
                {
                    pawn.Destroy();
                }
            }
            else if (!stock.TryAdd(thing, canMergeWithExistingStacks: false))
            {
                thing.Destroy();
            }
        }

        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Caravan caravan = playerNegotiator.GetCaravan();
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            if (thing is Pawn p)
            {
                caravan.AddPawn(p, addCarriedPawnToWorldPawnsIfAny: true);
                return;
            }
            Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null);
            if (pawn == null)
            {
                Log.Error("Could not find any pawn to give sold thing to.");
                thing.Destroy();
            }
            else if (!pawn.inventory.innerContainer.TryAdd(thing))
            {
                Log.Error("Could not add sold thing to inventory.");
                thing.Destroy();
            }
        }

        public virtual void TraderTrackerTick()
        {
            if (stock == null)
            {
                return;
            }
            for (int num = stock.Count - 1; num >= 0; num--)
            {
                if (stock[num] is Pawn pawn && pawn.Destroyed is false)
                {
                    stock.Remove(pawn);
                }
            }
            for (int num2 = stock.Count - 1; num2 >= 0; num2--)
            {
                if (stock[num2] is Pawn pawn2 && !pawn2.IsWorldPawn())
                {
                    Log.Error("Faction base has non-world-pawns in its stock. Removing...");
                    stock.Remove(pawn2);
                }
            }
        }

        public void TryDestroyStock()
        {
            if (stock == null)
            {
                return;
            }
            for (int num = stock.Count - 1; num >= 0; num--)
            {
                Thing thing = stock[num];
                stock.Remove(thing);
                if (!(thing is Pawn) && !thing.Destroyed)
                {
                    thing.Destroy();
                }
            }
            stock = null;
        }

        public bool ContainsPawn(Pawn p)
        {
            if (stock != null)
            {
                return stock.Contains(p);
            }
            return false;
        }

        protected virtual void RegenerateStock()
        {
            TryDestroyStock();
            stock = new ThingOwner<Thing>(this);
            everGeneratedStock = true;

            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.traderDef = TraderKind;
            parms.tile = Tile;
            parms.makingFaction = Faction;
            stock.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));

            for (int i = 0; i < stock.Count; i++)
            {
                Thing thing = stock[i];
                if (stock[i] is Pawn pawn)
                {
                    Find.WorldPawns.PassToWorld(pawn);
                }
            }
            lastStockGenerationTicks = Find.TickManager.TicksGame;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return stock;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void PostRemove()
        {
            base.PostRemove();
            pather.StopDead();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pather, "pather", this);
            Scribe_Values.Look(ref ticksToStay, "ticksToStay");
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                tmpSavedPawns.Clear();
                if (stock != null)
                {
                    for (int num = stock.Count - 1; num >= 0; num--)
                    {
                        if (stock[num] is Pawn item)
                        {
                            stock.Remove(item);
                            tmpSavedPawns.Add(item);
                        }
                    }
                }
            }
            Scribe_Collections.Look(ref tmpSavedPawns, "tmpSavedPawns", LookMode.Reference);
            Scribe_Deep.Look(ref stock, "stock");
            Scribe_Values.Look(ref lastStockGenerationTicks, "lastStockGenerationTicks", 0);
            Scribe_Values.Look(ref everGeneratedStock, "wasStockGeneratedYet", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
            {
                for (int i = 0; i < tmpSavedPawns.Count; i++)
                {
                    stock.TryAdd(tmpSavedPawns[i], canMergeWithExistingStacks: false);
                }
                tmpSavedPawns.Clear();
            }
        }
    }
}