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
    [StaticConstructorOnStartup]
    public class BarterDeal
    {
        public List<Tradeable> tradeablesPlayer = new List<Tradeable>();
        public List<Tradeable> tradeablesTrader = new List<Tradeable>();
        public List<Tradeable> AllTradeables => tradeablesPlayer.Concat(tradeablesTrader).ToList();

        public float Fairness
        {
            get
            {
                var playerSum = tradeablesPlayer.Sum(x => x.CurTotalCurrencyCostForDestination);
                var traderSum = tradeablesTrader.Sum(x => x.CurTotalCurrencyCostForSource);
                var total = playerSum + traderSum;
                var fairness = playerSum / total;
                return fairness;
            }
        }

        public static readonly Texture2D FairDealTexture = SolidColorMaterials.NewSolidColorTexture(Color.green);
        public static readonly Texture2D BadDealTexture = SolidColorMaterials.NewSolidColorTexture(Color.red);

        public Texture2D BarterFairnessColor(float fairness)
        {
            if (fairness < 0.5f)
            {
               return BadDealTexture;
            }
            return FairDealTexture;
        }

        public List<string> cannotSellReasons = new List<string>();

        public BarterDeal()
        {
            Reset();
        }

        public void Reset()
        {
            tradeablesPlayer.Clear();
            tradeablesTrader.Clear();
            cannotSellReasons.Clear();
            AddAllTradeables();
        }

        private void AddAllTradeables()
        {
            foreach (Thing item in TradeSession.trader.ColonyThingsWillingToBuy(TradeSession.playerNegotiator))
            {
                if (!TradeUtility.PlayerSellableNow(item, TradeSession.trader))
                {
                    continue;
                }
                AddToTradeables(item, Transactor.Colony);
            }
            foreach (Thing good in TradeSession.trader.Goods)
            {
                AddToTradeables(good, Transactor.Trader);
            }
        }

        private void AddToTradeables(Thing t, Transactor trans)
        {
            var tradeables = trans == Transactor.Colony ? tradeablesPlayer : tradeablesTrader;
            Tradeable tradeable = TransferableUtility.TradeableMatching(t, tradeables);
            bool initPrice = false;
            if (tradeable == null)
            {
                tradeable = ((!(t is Pawn)) ? new Tradeable() : new Tradeable_Pawn());
                tradeables.Add(tradeable);
                initPrice = true;
            }
            tradeable.AddThing(t, trans);
            if (initPrice)
            {
                Rand.PushState((TradeSession.trader as MerchantGuild).Tile);
                tradeable.InitPriceDataIfNeeded();
                if (trans == Transactor.Colony)
                {
                    Log.Message("Colony: " + tradeable.ThingDef + " BEFORE: " + tradeable.pricePlayerSell);
                    tradeable.pricePlayerSell *= Rand.Range(0.1f, 2f);
                    Log.Message("Colony: " + tradeable.ThingDef + " AFTER: " + tradeable.pricePlayerSell);
                }
                else
                {
                    Log.Message("Trader: " + tradeable.ThingDef + " BEFORE: " + tradeable.pricePlayerBuy);
                    tradeable.pricePlayerBuy *= Rand.Range(0.1f, 2f);
                    Log.Message("Trader: " + tradeable.ThingDef + " AFTER: " + tradeable.pricePlayerBuy);
                }
                Log.ResetMessageCount();
                Rand.PopState();
            }
        }

        public bool TryExecute(out bool actuallyTraded)
        {
            actuallyTraded = false;
            if (ModsConfig.IdeologyActive && (CanTradeDueToIdeology(ref actuallyTraded, AllTradeables) is false))
            {
                return false;
            }
            actuallyTraded = false;
            float num = 0f;

            Trade(ref actuallyTraded, ref num, AllTradeables);
            Reset();
            if (TradeSession.trader.Faction != null)
            {
                TradeSession.trader.Faction.Notify_PlayerTraded(num, TradeSession.playerNegotiator);
            }
            if (TradeSession.trader is Pawn pawn)
            {
                TaleRecorder.RecordTale(TaleDefOf.TradedWith, TradeSession.playerNegotiator, pawn);
            }
            if (actuallyTraded)
            {
                TradeSession.playerNegotiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Trade);
            }
            return true;
        }

        private bool BarterIsNotPossible()
        {
            return false;
        }

        private static bool CanTradeDueToIdeology(ref bool actuallyTraded, List<Tradeable> tradeables)
        {
            if (tradeables.Any((Tradeable x) => x.ActionToDo != 0 && x.ThingDef != null && x.ThingDef.IsNaturalOrgan))
            {
                HistoryEvent historyEvent = new HistoryEvent(HistoryEventDefOf.TradedOrgan, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer));
                if (!historyEvent.Notify_PawnAboutToDo())
                {
                    actuallyTraded = false;
                    return false;
                }
                Find.HistoryEventsManager.RecordEvent(historyEvent);
            }
            if (tradeables.Any((Tradeable x) => x.ActionToDo == TradeAction.PlayerSells && x.ThingDef != null && x.ThingDef.IsNaturalOrgan))
            {
                HistoryEvent historyEvent2 = new HistoryEvent(HistoryEventDefOf.SoldOrgan, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer));
                if (!historyEvent2.Notify_PawnAboutToDo())
                {
                    actuallyTraded = false;
                    return false;
                }
                Find.HistoryEventsManager.RecordEvent(historyEvent2);
            }
            return true;
        }

        private static void Trade(ref bool actuallyTraded, ref float num, List<Tradeable> tradeables)
        {
            foreach (Tradeable tradeable in tradeables)
            {
                if (tradeable.ActionToDo != 0)
                {
                    actuallyTraded = true;
                }
                if (tradeable.ActionToDo == TradeAction.PlayerSells)
                {
                    num += tradeable.CurTotalCurrencyCostForDestination;
                }
                tradeable.ResolveTrade();
            }
        }
    }
}