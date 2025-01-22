using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(TradeDeal), "CurrencyTradeable", MethodType.Getter)]
    public static class TradeDeal_CurrencyTradeable_Patch
    {
        public static void Postfix(TradeDeal __instance, ref Tradeable __result)
        {
            if (Find.WindowStack.IsOpen<Dialog_Barter>())
            {
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(Tradeable), "IsCurrency", MethodType.Getter)]
    public static class Tradeable_IsCurrency_Patch
    {
        public static void Postfix(Tradeable __instance, ref bool __result)
        {
            if (Find.WindowStack.IsOpen<Dialog_Barter>())
            {
                __result = false;
            }
        }
    }

    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Dialog_Barter : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

        private List<Tradeable> cachedTradeables;

        private TransferableSorterDef sorter1;

        private TransferableSorterDef sorter2;

        private bool playerIsCaravan;

        private List<Thing> playerCaravanAllPawnsAndItems;

        private bool massUsageDirty = true;

        private float cachedMassUsage;

        private bool massCapacityDirty = true;

        private float cachedMassCapacity;

        private string cachedMassCapacityExplanation;

        private bool tilesPerDayDirty = true;

        private float cachedTilesPerDay;

        private string cachedTilesPerDayExplanation;

        private bool daysWorthOfFoodDirty = true;

        private Pair<float, float> cachedDaysWorthOfFood;

        private bool foragedFoodPerDayDirty = true;

        private Pair<ThingDef, float> cachedForagedFoodPerDay;

        private string cachedForagedFoodPerDayExplanation;

        private bool visibilityDirty = true;

        private float cachedVisibility;

        private string cachedVisibilityExplanation;

        protected static readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

        protected static readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

        private static readonly Texture2D ShowSellableItemsIcon = ContentFinder<Texture2D>.Get("UI/Commands/SellableItems");

        private static readonly Texture2D GiftModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/GiftMode");

        private static readonly Texture2D TradeModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/TradeMode");

        public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

        private int Tile => TradeSession.playerNegotiator.Tile;

        private BiomeDef Biome => Find.WorldGrid[Tile].biome;

        private float MassUsage
        {
            get
            {
                if (massUsageDirty)
                {
                    massUsageDirty = false;
                    cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, IgnorePawnsInventoryMode.Ignore);
                }
                return cachedMassUsage;
            }
        }

        private float MassCapacity
        {
            get
            {
                if (massCapacityDirty)
                {
                    massCapacityDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    cachedMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, stringBuilder);
                    cachedMassCapacityExplanation = stringBuilder.ToString();
                }
                return cachedMassCapacity;
            }
        }

        private float TilesPerDay
        {
            get
            {
                if (tilesPerDayDirty)
                {
                    tilesPerDayDirty = false;
                    Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
                    StringBuilder stringBuilder = new StringBuilder();
                    cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, MassUsage, MassCapacity, Tile, (caravan != null && caravan.pather.Moving) ? caravan.pather.nextTile : (-1), stringBuilder);
                    cachedTilesPerDayExplanation = stringBuilder.ToString();
                }
                return cachedTilesPerDay;
            }
        }

        private Pair<float, float> DaysWorthOfFood
        {
            get
            {
                if (daysWorthOfFoodDirty)
                {
                    daysWorthOfFoodDirty = false;
                    float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Tile, IgnorePawnsInventoryMode.Ignore, Faction.OfPlayer);
                    cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Tile, IgnorePawnsInventoryMode.Ignore));
                }
                return cachedDaysWorthOfFood;
            }
        }

        private Pair<ThingDef, float> ForagedFoodPerDay
        {
            get
            {
                if (foragedFoodPerDayDirty)
                {
                    foragedFoodPerDayDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Biome, Faction.OfPlayer, stringBuilder);
                    cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
                }
                return cachedForagedFoodPerDay;
            }
        }

        private float Visibility
        {
            get
            {
                if (visibilityDirty)
                {
                    visibilityDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    cachedVisibility = CaravanVisibilityCalculator.VisibilityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, stringBuilder);
                    cachedVisibilityExplanation = stringBuilder.ToString();
                }
                return cachedVisibility;
            }
        }

        public override QuickSearchWidget CommonSearchWidget => quickSearchWidget;

        public Dialog_Barter(Pawn playerNegotiator, ITrader trader)
        {
            TradeSession.SetupWith(trader, playerNegotiator, false);
            SetupPlayerCaravanVariables();
            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            if (trader is PassingShip)
            {
                soundAmbient = SoundDefOf.RadioComms_Ambience;
            }
            commonSearchWidgetOffset.x += 18f;
            commonSearchWidgetOffset.y -= 18f;
            sorter1 = TransferableSorterDefOf.Category;
            sorter2 = TransferableSorterDefOf.MarketValue;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            quickSearchWidget.Reset();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            Pawn playerNegotiator = TradeSession.playerNegotiator;
            float level = playerNegotiator.health.capacities.GetLevel(PawnCapacityDefOf.Talking);
            float level2 = playerNegotiator.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
            if (level < 0.95f || level2 < 0.95f)
            {
                TaggedString text = ((!(level < 0.95f)) ? "NegotiatorHearingImpaired".Translate(playerNegotiator.LabelShort, playerNegotiator) : "NegotiatorTalkingImpaired".Translate(playerNegotiator.LabelShort, playerNegotiator));
                text += "\n\n" + "NegotiatorCapacityImpaired".Translate();
                Find.WindowStack.Add(new Dialog_MessageBox(text));
            }
            CacheTradeables();
        }

        private void CacheTradeables()
        {
            cachedTradeables = (from tr in TradeSession.deal.AllTradeables
                                where (tr.TraderWillTrade 
                                || !TradeSession.trader.TraderKind.hideThingsNotWillingToTrade)
                                where quickSearchWidget.filter.Matches(tr.Label)
                                orderby (!tr.TraderWillTrade) ? (-1) : 0 descending
                                select tr).ThenBy((Tradeable tr) => tr, sorter1.Comparer).ThenBy((Tradeable tr) => tr, sorter2.Comparer).ThenBy((Tradeable tr) => TransferableUIUtility.DefaultListOrderPriority(tr))
                .ThenBy((Tradeable tr) => tr.ThingDef.label)
                .ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out var qc) ? ((int)qc) : (-1))
                .ThenBy((Tradeable tr) => tr.AnyThing.HitPoints)
                .ToList();
            Log.Message("cachedTradeables: " + cachedTradeables.Select(x => x.ThingDef).ToStringSafeEnumerable());
            quickSearchWidget.noResultsMatched = !cachedTradeables.Any();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (playerIsCaravan)
            {
                CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, cachedMassCapacityExplanation, TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation), null, Tile, null, -9999f, new Rect(12f, 0f, inRect.width - 24f, 40f));
                inRect.yMin += 52f;
            }
            Widgets.BeginGroup(inRect);
            inRect = inRect.AtZero();
            TransferableUIUtility.DoTransferableSorters(sorter1, sorter2, delegate (TransferableSorterDef x)
            {
                sorter1 = x;
                CacheTradeables();
            }, delegate (TransferableSorterDef x)
            {
                sorter2 = x;
                CacheTradeables();
            });
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(new Rect(0f, 27f, inRect.width / 2f, inRect.height / 2f), "NegotiatorTradeDialogInfo".Translate(TradeSession.playerNegotiator.Name.ToStringFull, TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement).ToStringPercent()));
            float num = inRect.width - 590f;
            Rect rect = new Rect(num, 0f, inRect.width - num, 58f);
            Widgets.BeginGroup(rect);
            Text.Font = GameFont.Medium;
            Rect rect2 = new Rect(0f, 0f, rect.width / 2f, rect.height);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect2, Faction.OfPlayer.Name.Truncate(rect2.width));
            Rect rect3 = new Rect(rect.width / 2f, 0f, rect.width / 2f, rect.height);
            Text.Anchor = TextAnchor.UpperRight;
            string text = TradeSession.trader.TraderName;
            if (Text.CalcSize(text).x > rect3.width)
            {
                Text.Font = GameFont.Small;
                text = text.Truncate(rect3.width);
            }
            Widgets.Label(rect3, text);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(new Rect(rect.width / 2f, 27f, rect.width / 2f, rect.height / 2f), TradeSession.trader.TraderKind.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Text.Font = GameFont.Tiny;
            Rect rect4 = new Rect(rect.width / 2f - 100f - 30f, 0f, 200f, rect.height);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect4, "PositiveBuysNegativeSells".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            Widgets.EndGroup();
            float num2 = 0f;
            Rect mainRect = new Rect(0f, 58f + num2, inRect.width, inRect.height - 58f - 38f - num2 - 20f);
            FillMainRect(mainRect);
            Text.Font = GameFont.Small;
            Rect rect5 = new Rect(inRect.width / 2f - AcceptButtonSize.x / 2f, inRect.height - 55f, AcceptButtonSize.x, AcceptButtonSize.y);
            if (Widgets.ButtonText(rect5, "AcceptButton".Translate()))
            {
                Action action = delegate
                {
                    if (TradeSession.deal.TryExecute(out var actuallyTraded))
                    {
                        if (actuallyTraded)
                        {
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                            TradeSession.playerNegotiator.GetCaravan()?.RecacheImmobilizedNow();
                            Close(doCloseSound: false);
                        }
                        else
                        {
                            Close();
                        }
                    }
                };
                if (TradeSession.deal.DoesTraderHaveEnoughSilver())
                {
                    action();
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmTraderShortFunds".Translate(), action));
                }
                Event.current.Use();
            }
            if (Widgets.ButtonText(new Rect(rect5.x - 10f - OtherBottomButtonSize.x, rect5.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "ResetButton".Translate()))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                TradeSession.deal.Reset();
                CacheTradeables();
                CountToTransferChanged();
            }
            if (Widgets.ButtonText(new Rect(rect5.xMax + 10f, rect5.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "CancelButton".Translate()))
            {
                Close();
                Event.current.Use();
            }
            float y = OtherBottomButtonSize.y;
            Rect rect6 = new Rect(inRect.width - y, rect5.y, y, y);
            if (Widgets.ButtonImageWithBG(rect6, ShowSellableItemsIcon, new Vector2(32f, 32f)))
            {
                Find.WindowStack.Add(new Dialog_SellableItems(TradeSession.trader));
            }
            TooltipHandler.TipRegionByKey(rect6, "CommandShowSellableItemsDesc");
            Widgets.EndGroup();
        }

        public override void Close(bool doCloseSound = true)
        {
            DragSliderManager.ForceStop();
            base.Close(doCloseSound);
        }

        private void FillMainRect(Rect mainRect)
        {
            Text.Font = GameFont.Small;
            float height = 6f + (float)cachedTradeables.Count * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 6f;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;
            for (int i = 0; i < cachedTradeables.Count; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    int countToTransfer = cachedTradeables[i].CountToTransfer;
                    DrawTradeableRow(rect, cachedTradeables[i], num4);
                    if (countToTransfer != cachedTradeables[i].CountToTransfer)
                    {
                        CountToTransferChanged();
                    }
                }
                num += 30f;
                num4++;
            }
            Widgets.EndScrollView();
        }


        public static void DrawTradeableRow(Rect rect, Tradeable trad, int index)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Text.Font = GameFont.Small;
            Widgets.BeginGroup(rect);
            float width = rect.width;
            int num = trad.CountHeldBy(Transactor.Trader);
            if (num != 0 && trad.IsThing)
            {
                Rect rect2 = new Rect(width - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                Text.Anchor = TextAnchor.MiddleRight;
                Rect rect3 = rect2;
                rect3.xMin += 5f;
                rect3.xMax -= 5f;
                Widgets.Label(rect3, num.ToStringCached());
                TooltipHandler.TipRegionByKey(rect2, "TraderCount");
            }
            width -= 175f;
            Rect rect5 = new Rect(width - 350f, 0f, 240f, rect.height);

            TransferableUIUtility.DoCountAdjustInterface(rect5, trad, index, trad.GetMinimumToTransfer(),
                trad.GetMaximumToTransfer(), false);
            Log.Message(trad.ThingDef + " - trad.GetMinimumToTransfer(): " + trad.GetMinimumToTransfer()
                + " - trad.GetMaximumToTransfer(): " + trad.GetMaximumToTransfer());
            Log.ResetMessageCount();
            width -= 240f;
            int num2 = trad.CountHeldBy(Transactor.Colony);
            if (num2 != 0 || trad.IsCurrency)
            {
                Rect rect6 = new Rect(width - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect7))
                {
                    Widgets.DrawHighlight(rect7);
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect8 = rect7;
                rect8.xMin += 5f;
                rect8.xMax -= 5f;
                Widgets.Label(rect8, num2.ToStringCached());
                TooltipHandler.TipRegionByKey(rect7, "ColonyCount");
            }
            width -= 175f;
            TransferableUIUtility.DoExtraIcons(trad, rect, ref width);
            if (ModsConfig.IdeologyActive)
            {
                TransferableUIUtility.DrawCaptiveTradeInfo(trad, TradeSession.trader, rect, ref width);
            }
            Rect idRect = new Rect(0f, 0f, width, rect.height);
            TransferableUIUtility.DrawTransferableInfo(trad, idRect, trad.TraderWillTrade ? Color.white : TradeUI.NoTradeColor);
            GenUI.ResetLabelAlign();
            Widgets.EndGroup();
        }

        public override bool CausesMessageBackground()
        {
            return true;
        }

        private void SetupPlayerCaravanVariables()
        {
            Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
            if (caravan != null)
            {
                playerIsCaravan = true;
                playerCaravanAllPawnsAndItems = new List<Thing>();
                List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
                for (int i = 0; i < pawnsListForReading.Count; i++)
                {
                    playerCaravanAllPawnsAndItems.Add(pawnsListForReading[i]);
                }
                playerCaravanAllPawnsAndItems.AddRange(CaravanInventoryUtility.AllInventoryItems(caravan));
                caravan.Notify_StartedTrading();
            }
            else
            {
                playerIsCaravan = false;
            }
        }

        private void CountToTransferChanged()
        {
            massUsageDirty = true;
            massCapacityDirty = true;
            tilesPerDayDirty = true;
            daysWorthOfFoodDirty = true;
            foragedFoodPerDayDirty = true;
            visibilityDirty = true;
        }

        public override void Notify_CommonSearchChanged()
        {
            CacheTradeables();
        }
    }
}