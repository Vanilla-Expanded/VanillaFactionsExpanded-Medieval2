﻿using HarmonyLib;
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
        public static void Postfix(BarterDeal __instance, ref Tradeable __result)
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
        private Vector2 scrollPositionPlayer = Vector2.zero;

        private Vector2 scrollPositionTrader = Vector2.zero;

        private Vector2 scrollPositionPlayerSummary = Vector2.zero;

        private Vector2 scrollPositionTraderSummary = Vector2.zero;

        private QuickSearchWidget quickSearchWidgetTrader;

        private QuickSearchWidget quickSearchWidgetPlayer;

        private List<Tradeable> cachedTradeablesPlayer;

        private List<Tradeable> cachedTradeablesTrader;

        private TransferableSorterDef sorter1;

        private TransferableSorterDef sorter2;

        private static readonly Texture2D WhiteIndicator = ContentFinder<Texture2D>.Get("UI/WhiteIndicator_Up");

        private BarterDeal deal;

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

        public override Vector2 InitialSize => new Vector2(1280f, UI.screenHeight);

        private int Tile => TradeSession.playerNegotiator.Tile;

        private BiomeDef Biome => Find.WorldGrid[Tile].biome;

        private float MassUsage
        {
            get
            {
                if (massUsageDirty)
                {
                    massUsageDirty = false;
                    cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, IgnorePawnsInventoryMode.Ignore);
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
                    cachedMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, stringBuilder);
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
                    cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, MassUsage, MassCapacity, Tile,
                        (caravan != null && caravan.pather.Moving) ? caravan.pather.nextTile : (-1),false, stringBuilder);
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
                    float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, Tile, IgnorePawnsInventoryMode.Ignore, Faction.OfPlayer);
                    cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, Tile, IgnorePawnsInventoryMode.Ignore));
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
                    cachedForagedFoodPerDay.first = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, Biome, Faction.OfPlayer, stringBuilder).food;
                    cachedForagedFoodPerDay.second = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, Biome, Faction.OfPlayer, stringBuilder).perDay;

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
                    cachedVisibility = CaravanVisibilityCalculator.VisibilityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, deal.AllTradeables, stringBuilder);
                    cachedVisibilityExplanation = stringBuilder.ToString();
                }
                return cachedVisibility;
            }
        }

        public Dialog_Barter(Pawn playerNegotiator, ITrader trader)
        {
            TradeSession.SetupWith(trader, playerNegotiator, false);
            SetupPlayerCaravanVariables();
            deal = new BarterDeal();
            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            if (trader is PassingShip)
            {
                soundAmbient = SoundDefOf.RadioComms_Ambience;
            }
            sorter1 = TransferableSorterDefOf.Category;
            sorter2 = TransferableSorterDefOf.MarketValue;
            QuickSearchWidget.instanceCounter++;
            quickSearchWidgetTrader = new QuickSearchWidget();
            QuickSearchWidget.instanceCounter++;
            quickSearchWidgetPlayer = new QuickSearchWidget();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            quickSearchWidgetPlayer.Reset();
            quickSearchWidgetTrader.Reset();
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
            CacheTradeablesPlayer();
            CacheTradeablesTrader();
        }

        private void CacheTradeablesPlayer()
        {
            CacheTradeables(ref cachedTradeablesPlayer, deal.tradeablesPlayer, quickSearchWidgetPlayer);
        }

        private void CacheTradeablesTrader()
        {
            CacheTradeables(ref cachedTradeablesTrader, deal.tradeablesTrader, quickSearchWidgetTrader);
        }

        private void CacheTradeables(ref List<Tradeable> cachedTradeables, List<Tradeable> tradeables,
            QuickSearchWidget quickSearchWidget)
        {
            cachedTradeables = (from tr in tradeables
                                where quickSearchWidget.filter.Matches(tr.Label)
                                orderby (!tr.TraderWillTrade) ? (-1) : 0 descending
                                select tr).ThenBy((Tradeable tr) => tr, sorter1.Comparer)
                      .ThenBy((Tradeable tr) => tr, sorter2.Comparer).ThenBy((Tradeable tr) =>
                      TransferableUIUtility.DefaultListOrderPriority(tr))
                        .ThenBy((Tradeable tr) => tr.ThingDef.label)
                        .ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out var qc) ? ((int)qc) : (-1))
                        .ThenBy((Tradeable tr) => tr.AnyThing.HitPoints)
                        .ToList();
            quickSearchWidget.noResultsMatched = !cachedTradeables.Any();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (playerIsCaravan)
            {
                CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, cachedMassCapacityExplanation, TilesPerDay, cachedTilesPerDayExplanation, (DaysWorthOfFood.first, DaysWorthOfFood.second), (ForagedFoodPerDay.first, ForagedFoodPerDay.second), cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation), null, Tile, null, -9999f, new Rect(12f, 0f, inRect.width - 24f, 40f));
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
            float num = inRect.width - 750f;
            Rect rect = new Rect(num, 0f, inRect.width - num, 58f);
            Widgets.BeginGroup(rect);
            Text.Font = GameFont.Medium;
            var barterWindowWidth = InitialSize.x / 2;
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

            Widgets.EndGroup();
            barterWindowWidth -= 18f;
            Rect mainRect = new Rect(0f, 48f, barterWindowWidth, inRect.height - 300);
            FillMainRect(mainRect, cachedTradeablesPlayer, ref scrollPositionPlayer, false, true);

            mainRect.x += barterWindowWidth;
            FillMainRect(mainRect, cachedTradeablesTrader, ref scrollPositionTrader, true, true);

            var playerSells = cachedTradeablesPlayer.Where(x => x.ActionToDo == TradeAction.PlayerSells).ToList();
            var playerBuys = cachedTradeablesTrader.Where(x => x.ActionToDo == TradeAction.PlayerBuys).ToList();
            barterWindowWidth -= 270;
            var summaryRect = new Rect(0f, mainRect.yMax + 15, barterWindowWidth, 170);
            DrawBarterSummary(playerSells, ref summaryRect, ref scrollPositionPlayerSummary, Faction.OfPlayer.Name);
            var fairnessRect = new Rect(summaryRect.xMax + 30, summaryRect.y, inRect.width - ((barterWindowWidth + 30) * 2f), summaryRect.height);
            var fairness = deal.Fairness;
            FillBarterFairness(fairnessRect, fairness);
            summaryRect.x = inRect.width - barterWindowWidth;
            DrawBarterSummary(playerBuys, ref summaryRect, ref scrollPositionTraderSummary, TradeSession.trader.TraderName);

            Text.Font = GameFont.Small;
            Rect rect5 = new Rect(inRect.width / 2f - AcceptButtonSize.x / 2f, inRect.height - AcceptButtonSize.y,
                AcceptButtonSize.x, AcceptButtonSize.y);
            GUI.color = fairness < 0.5f ? Color.grey : Color.white;
            if (Widgets.ButtonText(rect5, "AcceptButton".Translate()) && fairness >= 0.5f)
            {
                Action action = delegate
                {
                    if (deal.TryExecute(out var actuallyTraded))
                    {
                        if (actuallyTraded)
                        {
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                            TradeSession.playerNegotiator.GetCaravan()?.RecacheInventory();
                            Close(doCloseSound: false);
                        }
                        else
                        {
                            Close();
                        }
                    }
                };
                action();
                Event.current.Use();
            }

            GUI.color = Color.white;
            if (Widgets.ButtonText(new Rect(rect5.x - 10f - OtherBottomButtonSize.x, rect5.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "ResetButton".Translate()))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                deal.Reset();
                CacheTradeables();
                CountToTransferChanged();
            }

            if (Widgets.ButtonText(new Rect(rect5.xMax + 10f, rect5.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "CancelButton".Translate()))
            {
                Close();
                Event.current.Use();
            }
            var playerSearch = new Rect(0f, inRect.height - 24f, 200, 24f);
            quickSearchWidgetPlayer.OnGUI(playerSearch, CacheTradeablesPlayer);
            var traderSearch = new Rect(inRect.width - 200, inRect.height - 24f, 200, 24f);
            quickSearchWidgetTrader.OnGUI(traderSearch, CacheTradeablesTrader);
            Widgets.EndGroup();
        }

        private void DrawBarterSummary(List<Tradeable> playerSells, ref Rect summaryRect, ref Vector2 scrollPosition, string factionName)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(summaryRect.x, summaryRect.y, summaryRect.width, 30f), factionName);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            summaryRect.y += 30f;
            GUI.color = Color.grey;
            Widgets.DrawLineHorizontal(summaryRect.x, summaryRect.y, summaryRect.width);
            GUI.color = Color.white;
            FillMainRect(summaryRect, playerSells, ref scrollPosition, false, false);
            summaryRect.y -= 30f;
        }

        public void FillBarterFairness(Rect rect, float fairness)
        {
            GUI.color = Color.grey;
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            rect.yMin += 15;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "VFEM2_BarterFairness".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            rect.yMin += 40f;
            rect.height = 24;
            Widgets.FillableBar(rect, fairness, deal.BarterFairnessColor(fairness), Widgets.DefaultBarBgTex, false);
            rect.yMin += 30f;
            var triangeRect = new Rect(rect.x + rect.width / 2 - 12, rect.y - 5, 24, 24);
            Widgets.DrawTextureFitted(triangeRect, WhiteIndicator, 1f);
            GUI.color = Color.grey;
            var explanationRect = new Rect(rect.x, rect.y + 25, rect.width, 80);
            Widgets.Label(explanationRect, "VFEM2_BarterExplanation".Translate());
            GUI.color = Color.white;
        }

        public override void Close(bool doCloseSound = true)
        {
            DragSliderManager.ForceStop();
            base.Close(doCloseSound);
        }

        private void FillMainRect(Rect mainRect, List<Tradeable> cachedTradeables, ref Vector2 scrollPosition,
            bool isTrader, bool interactive)
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
                    DrawTradeableRow(rect, cachedTradeables[i], num4, isTrader, interactive);
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

        public static void DrawTradeableRow(Rect rect, Tradeable trad, int index, bool isTrader, bool interactive)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Text.Font = GameFont.Small;
            Widgets.BeginGroup(rect);
            float width = rect.width;
            bool isNotHumanlike = (trad.AnyThing is not Pawn pawn || pawn.RaceProps.Humanlike is false);
            if (interactive)
            {
                int num = isTrader ? trad.CountHeldBy(Transactor.Trader) : trad.CountHeldBy(Transactor.Colony);
                string traderCountTooltip = isTrader ? "TraderCount" : "ColonyCount";

                if (num != 0 && trad.IsThing && isNotHumanlike)
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
                    TooltipHandler.TipRegionByKey(rect2, traderCountTooltip);
                    width -= rect2.width;
                }
                width -= 230;

                Rect rect5 = new Rect(width, 0f, 255f, rect.height);
                width += 20;
                if (isNotHumanlike)
                {
                    DoCountAdjustInterfaceInternal(rect5, trad, index, trad.GetMinimumToTransfer(), trad.GetMaximumToTransfer(), false, false);
                    //TransferableUIUtility.DoCountAdjustInterface(rect5, trad, index, trad.GetMinimumToTransfer(), trad.GetMaximumToTransfer(), false, readOnly: isTrader);
                }
                else
                {
                    rect5.x += 30;
                    DoCountAdjustInterfaceForHumanlike(rect5, trad, index, trad.GetMinimumToTransfer(), trad.GetMaximumToTransfer(), false);
                }
            }
            if (isNotHumanlike is false)
            {
                rect.width -= 60;
                width -= 80;
            }
            TransferableUIUtility.DoExtraIcons(trad, rect, ref width);
            if (ModsConfig.IdeologyActive)
            {
                TransferableUIUtility.DrawCaptiveTradeInfo(trad, TradeSession.trader, rect, ref width);
            }
            Rect idRect = new Rect(0f, 0f, width, rect.height);
            DrawTransferableInfo(trad, idRect, trad.TraderWillTrade ? Color.white : TradeUI.NoTradeColor, interactive is false);
            GenUI.ResetLabelAlign();
            Widgets.EndGroup();
        }


        public static void DoCountAdjustInterfaceForHumanlike(Rect rect, Transferable trad, int index, int min, int max, bool flash = false, List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = null, bool readOnly = false)
        {
            TransferableUIUtility.stoppingPoints.Clear();
            if (extraStoppingPoints != null)
            {
                TransferableUIUtility.stoppingPoints.AddRange(extraStoppingPoints);
            }
            for (int num = TransferableUIUtility.stoppingPoints.Count - 1; num >= 0; num--)
            {
                if (TransferableUIUtility.stoppingPoints[num].threshold != 0 && (TransferableUIUtility.stoppingPoints[num].threshold <= min || TransferableUIUtility.stoppingPoints[num].threshold >= max))
                {
                    TransferableUIUtility.stoppingPoints.RemoveAt(num);
                }
            }
            bool flag = false;
            for (int i = 0; i < TransferableUIUtility.stoppingPoints.Count; i++)
            {
                if (TransferableUIUtility.stoppingPoints[i].threshold == 0)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                TransferableUIUtility.stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(0, "0", "0"));
            }
            DoCountAdjustInterfaceInternal(rect, trad, index, min, max, flash, readOnly);
        }

        private static void DoCountAdjustInterfaceInternal(Rect rect, Transferable trad, int index, int min, int max, bool flash, bool readOnly)
        {
            rect = rect.Rounded();
            Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();
            if (flash)
            {
                GUI.DrawTexture(rect2, TransferableUIUtility.FlashTex);
            }
            bool flag = trad is TransferableOneWay { HasAnyThing: not false } transferableOneWay && transferableOneWay.AnyThing is Pawn && transferableOneWay.MaxCount == 1;
            if (!trad.Interactive || readOnly)
            {
                if (flag)
                {
                    bool checkOn = trad.CountToTransfer != 0;
                    Widgets.Checkbox(rect2.position, ref checkOn, 24f, disabled: true);
                }
                else
                {
                    GUI.color = ((trad.CountToTransfer == 0) ? TransferableUIUtility.ZeroCountColor : Color.white);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect2, trad.CountToTransfer.ToStringCached());
                }
            }
            else if (flag)
            {
                bool flag2 = trad.CountToTransfer != 0;
                bool checkOn2 = flag2;
                Widgets.Checkbox(rect2.position, ref checkOn2, 24f, disabled: false, paintable: true);
                if (checkOn2 != flag2)
                {
                    if (checkOn2)
                    {
                        trad.AdjustTo(trad.GetMaximumToTransfer());
                    }
                    else
                    {
                        trad.AdjustTo(trad.GetMinimumToTransfer());
                    }
                }
            }
            else
            {
                Rect rect3 = rect2.ContractedBy(2f);
                rect3.xMax -= 15f;
                rect3.xMin += 16f;
                int val = trad.CountToTransfer;
                string buffer = trad.EditBuffer;
                Widgets.TextFieldNumeric(rect3, ref val, ref buffer, min, max);
                trad.AdjustTo(val);
                trad.EditBuffer = buffer;
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            if (trad.Interactive && !flag && !readOnly)
            {
                TransferablePositiveCountDirection positiveCountDirection = trad.PositiveCountDirection;
                int num = ((positiveCountDirection == TransferablePositiveCountDirection.Source) ? 1 : (-1));
                int num2 = GenUI.CurrentAdjustmentMultiplier();
                bool flag3 = trad.GetRange() == 1;
                if (trad.CanAdjustBy(num * num2).Accepted)
                {
                    Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
                    if (Widgets.ButtonText(rect4, "<"))
                    {
                        trad.AdjustBy(num * num2);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    if (!flag3)
                    {
                        string label = "<<";
                        int? num3 = null;
                        int num4 = 0;
                        for (int i = 0; i < TransferableUIUtility.stoppingPoints.Count; i++)
                        {
                            TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint = TransferableUIUtility.stoppingPoints[i];
                            if (positiveCountDirection == TransferablePositiveCountDirection.Source)
                            {
                                if (trad.CountToTransfer < transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold < num4 || !num3.HasValue))
                                {
                                    label = transferableCountToTransferStoppingPoint.leftLabel;
                                    num3 = transferableCountToTransferStoppingPoint.threshold;
                                }
                            }
                            else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold > num4 || !num3.HasValue))
                            {
                                label = transferableCountToTransferStoppingPoint.leftLabel;
                                num3 = transferableCountToTransferStoppingPoint.threshold;
                            }
                        }
                        rect4.x -= rect4.width;
                        if (Widgets.ButtonText(rect4, label, active: Mouse.IsOver(rect4)))
                        {
                            if (num3.HasValue)
                            {
                                trad.AdjustTo(num3.Value);
                            }
                            else if (num == 1)
                            {
                                trad.AdjustTo(trad.GetMaximumToTransfer());
                            }
                            else
                            {
                                trad.AdjustTo(trad.GetMinimumToTransfer());
                            }
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        }
                    }
                }
                if (trad.CanAdjustBy(-num * num2).Accepted)
                {
                    Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
                    if (Widgets.ButtonText(rect5, ">"))
                    {
                        trad.AdjustBy(-num * num2);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    if (!flag3)
                    {
                        string label2 = ">>";
                        int? num5 = null;
                        int num6 = 0;
                        for (int j = 0; j < TransferableUIUtility.stoppingPoints.Count; j++)
                        {
                            TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint2 = TransferableUIUtility.stoppingPoints[j];
                            if (positiveCountDirection == TransferablePositiveCountDirection.Destination)
                            {
                                if (trad.CountToTransfer < transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold < num6 || !num5.HasValue))
                                {
                                    label2 = transferableCountToTransferStoppingPoint2.rightLabel;
                                    num5 = transferableCountToTransferStoppingPoint2.threshold;
                                }
                            }
                            else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold > num6 || !num5.HasValue))
                            {
                                label2 = transferableCountToTransferStoppingPoint2.rightLabel;
                                num5 = transferableCountToTransferStoppingPoint2.threshold;
                            }
                        }
                        rect5.x += rect5.width;
                        if (Widgets.ButtonText(rect5, label2, active: Mouse.IsOver(rect5)))
                        {
                            if (num5.HasValue)
                            {
                                trad.AdjustTo(num5.Value);
                            }
                            else if (num == 1)
                            {
                                trad.AdjustTo(trad.GetMinimumToTransfer());
                            }
                            else
                            {
                                trad.AdjustTo(trad.GetMaximumToTransfer());
                            }
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                    }
                }
            }
            if (trad.CountToTransfer != 0)
            {
                Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TransferableUIUtility.TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TransferableUIUtility.TradeArrow.height / 2), TransferableUIUtility.TradeArrow.width, TransferableUIUtility.TradeArrow.height);
                TransferablePositiveCountDirection positiveCountDirection2 = trad.PositiveCountDirection;
                if ((positiveCountDirection2 == TransferablePositiveCountDirection.Source && trad.CountToTransfer > 0) || (positiveCountDirection2 == TransferablePositiveCountDirection.Destination && trad.CountToTransfer < 0))
                {
                    position.x += position.width;
                    position.width *= -1f;
                }
                GUI.DrawTexture(position, TransferableUIUtility.TradeArrow);
            }
        }

        public static void DrawTransferableInfo(Transferable trad, Rect idRect, Color labelColor,
            bool drawCountToTransfer)
        {
            if (!trad.HasAnyThing && trad.IsThing)
            {
                return;
            }
            if (Mouse.IsOver(idRect))
            {
                Widgets.DrawHighlight(idRect);
            }
            Rect rect = new Rect(0f, 0f, 27f, 27f);
            if (trad.IsThing)
            {
                try
                {
                    Widgets.ThingIcon(rect, trad.AnyThing);
                }
                catch (Exception ex)
                {
                    Log.Error("Exception drawing thing icon for " + trad.AnyThing.def.defName + ": " + ex.ToString());
                }
            }
            else
            {
                trad.DrawIcon(rect);
            }
            if (trad.IsThing)
            {
                Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(80f, 0f, idRect.width - 80f, idRect.height);
            Text.WordWrap = false;
            GUI.color = labelColor;
            var label = drawCountToTransfer ? Mathf.Abs(trad.CountToTransfer) + " " + trad.LabelCap : trad.LabelCap;
            Widgets.Label(rect2, label);
            GUI.color = Color.white;
            Text.WordWrap = true;
            if (!Mouse.IsOver(idRect))
            {
                return;
            }
            Transferable localTrad = trad;
            TooltipHandler.TipRegion(idRect, new TipSignal(delegate
            {
                if (!localTrad.HasAnyThing && localTrad.IsThing)
                {
                    return "";
                }
                string text = localTrad.LabelCap;
                string tipDescription = localTrad.TipDescription;
                if (localTrad.AnyThing is Book)
                {
                    text = tipDescription;
                }
                else if (!tipDescription.NullOrEmpty())
                {
                    text = text + ": " + tipDescription + TransferableUIUtility.ContentSourceDescription(localTrad.AnyThing);
                }
                CompIngredients compIngredients;
                if ((compIngredients = localTrad.AnyThing.TryGetComp<CompIngredients>()) != null)
                {
                    text = text + "\n\n" + compIngredients.CompInspectStringExtra();
                }
                return text;
            }, localTrad.GetHashCode()));
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
    }
}