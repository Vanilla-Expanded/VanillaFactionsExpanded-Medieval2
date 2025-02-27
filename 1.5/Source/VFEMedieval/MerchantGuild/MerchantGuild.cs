using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMedieval
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class MerchantGuild : MapParent, ILoadReferenceable, ITrader, IThingHolder
    {
        private Material cachedMat;

        public MerchantGuild_PathFollower pather;

        public MerchantGuild_Tweener tweener;

        public int TicksPerMove => 3300;

        public override Vector3 DrawPos => tweener.TweenedPos;

        private ThingOwner<Thing> stock;

        private int lastStockGenerationTicks = -1;

        private bool everGeneratedStock;

        public static readonly Texture2D AttackCommand = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");

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

        public override Texture2D ExpandingIcon => pather.Moving ? base.ExpandingIcon : ContentFinder<Texture2D>.Get(base.def.texture);

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

        public bool CanTradeNow => Faction.HostileTo(Faction.OfPlayer) is false;

        public float TradePriceImprovementOffsetForPlayer => 0f;

        public TradeCurrency TradeCurrency => TradeCurrency.Silver;

        private int ticksToStay;

        public override void PostAdd()
        {
            base.PostAdd();
            if (Rand.Bool)
            {
                StayInPlace();
            }
            else
            {
                TryStartPathing();
            }
        }

        public override string GetInspectString()
        {
            if (pather.MovingNow is false)
            {
                return "VFEM2_MerchantCaravanStayPeriod".Translate(ticksToStay.ToStringTicksToPeriod());
            }
            return "";
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (CanTradeNow && GuildVisitedNow(caravan) == this)
            {
                yield return BarterCommand(caravan, base.Faction, TraderKind);
            }
            foreach (Gizmo caravanGizmo in base.GetCaravanGizmos(caravan))
            {
                yield return caravanGizmo;
            }
            if (Attackable)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.icon = AttackCommand;
                command_Action.defaultLabel = "CommandAttackSettlement".Translate();
                command_Action.defaultDesc = "VFEM2_CommandAttackMerchantGuildDesc".Translate();
                command_Action.action = delegate
                {
                    this.Attack(caravan);
                };
                yield return command_Action;
            }
        }

        public void Attack(Caravan caravan)
        {
            if (HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    AttackNow(caravan);
                }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            }
            else
            {
                AttackNow(caravan);
            }
        }

        private void AttackNow(Caravan caravan)
        {
            bool mapWasGenerated = !HasMap;
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(Tile, null);
            CustomGenOption genOption = this.Faction.def.GetModExtension<CustomGenOption>();

            if (genOption != null)
            {
                IntVec3 generationLocation = map.Center;
                if (generationLocation.Fogged(map) && CellFinder.TryFindRandomSpawnCellForPawnNear(generationLocation, map, out var result,
                    extraValidator: (IntVec3 x) => x.Fogged(Map) is false))
                {
                    generationLocation = result;
                }
                var lord = LordMaker.MakeNewLord(Faction, new LordJob_DefendBase(Faction, generationLocation), map, null);

                if (pather.Moving)
                {
                    GeneratePawns(map, genOption, generationLocation, lord);
                }
                else
                {
                    genOption.Generate(generationLocation, map);
                }
                GenerateLoot(map, generationLocation, lord);
            }

            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString letterText = "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(this, ref letterText);
            if (mapWasGenerated)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(map.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
            }
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, Faction);
            CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
            Find.GoodwillSituationManager.RecalculateAll(canSendHostilityChangedLetter: true);
        }

        private void GeneratePawns(Map map, CustomGenOption genOption, IntVec3 generationLocation, Lord lord)
        {
            ResolveParams rp = default;
            rp.faction = this.Faction;

            int width = 50;
            int height = 50;

            CellRect rect = CellRect.CenteredOn(generationLocation, width, height);
            rect.ClipInsideMap(map);
            rp.rect = rect;
            BaseGen.globalSettings.map = map;
            rp.singlePawnLord = lord;
            rp.pawnGroupKindDef = PawnGroupKindDefOf.Settlement;
            rp.pawnGroupMakerParams = new PawnGroupMakerParms
            {
                tile = map.Tile,
                faction = this.Faction,
                points = new FloatRange(1150f, 1600f).RandomInRange,
                groupKind = PawnGroupKindDefOf.Settlement,
                inhabitants = true,
                seed = Rand.Range(0, int.MaxValue)
            };
            BaseGen.symbolStack.Push("pawnGroup", rp, null);
            BaseGen.Generate();
        }


        public void GenerateLoot(Map map, IntVec3 spawnPos, Lord lord)
        {
            PawnGroupMakerParms parms = new PawnGroupMakerParms();
            parms.tile = map.Tile;
            parms.faction = this.Faction;
            parms.groupKind = PawnGroupKindDefOf.Trader;
            parms.points = 1f;

            PawnGroupMaker groupMaker = Faction.def.pawnGroupMakers.First(x => x.kindDef == parms.groupKind);
            PawnKindDef carrierKind = groupMaker.carriers.Where(x => map.Biome.IsPackAnimalAllowed(x.kind.race)).RandomElementByWeight(x => x.selectionWeight).kind;

            List<Pawn> outPawns = new List<Pawn>();
            List<Thing> wares = new List<Thing>();

            ThingSetMakerParams wareParams = default(ThingSetMakerParams);
            wareParams.traderDef = this.TraderKind;
            wareParams.tile = map.Tile;
            wareParams.makingFaction = this.Faction;
            if (stock is null)
            {
                RegenerateStock();
            }
            List<Thing> fullWares = stock.Where(x => x is not Pawn).InRandomOrder().ToList();
            List<Pawn> slavesOrAnimals = stock.OfType<Pawn>().ToList();
            stock = null;
            float waresToSpawnFactor = 0.15f;
            if (fullWares.Any())
            {
                int wareCountToTake = Mathf.Max(1, Mathf.RoundToInt(fullWares.Count * waresToSpawnFactor));
                for (int i = 0; i < wareCountToTake && i < fullWares.Count; i++)
                {
                    Thing ware = fullWares[i].SplitOff(Mathf.Max(1, (int)(fullWares[i].stackCount * waresToSpawnFactor)));
                    wares.Add(ware);
                }
            }

            int numCarriers = Mathf.Max(1, Mathf.CeilToInt((float)wares.Count / 8f));

            List<Pawn> carriers = new List<Pawn>();
            int wareIndex = 0;
            for (int j = 0; j < numCarriers; j++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(carrierKind, parms.faction, PawnGenerationContext.NonPlayer, fixedIdeo: parms.ideo, tile: parms.tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: parms.inhabitants));
                carriers.Add(pawn);
                outPawns.Add(pawn);
                if (wareIndex < wares.Count)
                {
                    pawn.inventory.innerContainer.TryAdd(wares[wareIndex]);
                    wareIndex++;
                }
            }

            int slavesOrAnimalsCountToTake = Mathf.Max(0, Mathf.RoundToInt(slavesOrAnimals.Count * waresToSpawnFactor));
            List<Pawn> selectedSlavesOrAnimals = slavesOrAnimals.InRandomOrder().Take(slavesOrAnimalsCountToTake).ToList();
            foreach (var pawn in selectedSlavesOrAnimals)
            {
                outPawns.Add(pawn);
            }

            for (; wareIndex < wares.Count; wareIndex++)
            {
                carriers.RandomElement().inventory.innerContainer.TryAdd(wares[wareIndex]);
            }
            foreach (Pawn pawn in outPawns)
            {
                if (pawn.Faction != parms.faction)
                {
                    pawn.SetFaction(parms.faction);
                }
                if (CellFinder.TryFindRandomSpawnCellForPawnNear(spawnPos, map, out IntVec3 spawnCell, firstTryWithRadius: 30) is false)
                {
                    spawnCell = DropCellFinder.TradeDropSpot(map);
                    if (spawnCell.IsValid is false)
                    {
                        spawnCell = map.AllCells.Where(x => x.WalkableBy(map, pawn) && x.Roofed(map) is false
                            && x.GetFirstPawn(map) is null).RandomElement();
                    }
                }
                GenSpawn.Spawn(pawn, spawnCell, map);
                lord.AddPawn(pawn);
                Find.LetterStack.ReceiveLetter("test", "test", LetterDefOf.NeutralEvent, new LookTargets(pawn));
            }
        }

        public static MerchantGuild GuildVisitedNow(Caravan caravan)
        {
            if (!caravan.Spawned || caravan.pather.Moving)
            {
                return null;
            }
            List<MerchantGuild> bases = Find.WorldObjects.AllWorldObjects.OfType<MerchantGuild>().ToList();
            for (int i = 0; i < bases.Count; i++)
            {
                var settlement = bases[i];
                if (settlement.Tile == caravan.Tile)
                {
                    return settlement;
                }
            }
            return null;
        }

        public static Command_Action BarterCommand(Caravan caravan, Faction faction = null, TraderKindDef trader = null)
        {
            Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, trader);
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "VFEM2_CommandBarter".Translate();
            command_Action.defaultDesc = "VFEM2_CommandBarterDesc".Translate();
            command_Action.icon = CaravanVisitUtility.TradeCommandTex;
            command_Action.action = delegate
            {
                var guild = GuildVisitedNow(caravan);
                if (guild != null && guild.CanTradeNow)
                {
                    Find.WindowStack.Add(new Dialog_Barter(bestNegotiator, guild));
                    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(guild.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
                }
            };
            if (bestNegotiator == null)
            {
                if (trader != null && trader.permitRequiredForTrading != null && !caravan.PawnsListForReading.Any((Pawn p) => p.royalty != null && p.royalty.HasPermit(trader.permitRequiredForTrading, faction)))
                {
                    command_Action.Disable("CommandTradeFailNeedPermit".Translate(trader.permitRequiredForTrading.LabelCap));
                }
                else
                {
                    command_Action.Disable("CommandTradeFailNoNegotiator".Translate());
                }
            }
            if (bestNegotiator != null && bestNegotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                command_Action.Disable("CommandTradeFailSocialDisabled".Translate());
            }
            return command_Action;
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
            foreach (FloatMenuOption floatMenuOption3 in CaravanArrivalAction_Barter.GetFloatMenuOptions(caravan, this))
            {
                yield return floatMenuOption3;
            }
            foreach (FloatMenuOption floatMenuOption4 in CaravanArrivalAction_AttackMerchantGuild.GetFloatMenuOptions(caravan, this))
            {
                yield return floatMenuOption4;
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
            if (HasMap is false)
            {
                pather.PatherTick();
                if (this.IsHashIntervalTick(30))
                {
                    tweener.TweenerTick();
                }
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
                        StayInPlace();
                    }
                }
            }
        }

        public void StayInPlace()
        {
            ticksToStay = (int)(GenDate.TicksPerDay * Rand.Range(5f, 30f));
            pather.StopDead();
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
                Find.WorldPawns.PassToWorld(p);
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
            lastStockGenerationTicks = Find.TickManager.TicksGame;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return stock;
        }

        public override void GetChildHolders(List<IThingHolder> outChildren)
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
            Scribe_Deep.Look(ref stock, "stock");
            Scribe_Values.Look(ref lastStockGenerationTicks, "lastStockGenerationTicks", 0);
            Scribe_Values.Look(ref everGeneratedStock, "wasStockGeneratedYet", defaultValue: false);
        }

        public virtual bool Attackable => true;
    }
}