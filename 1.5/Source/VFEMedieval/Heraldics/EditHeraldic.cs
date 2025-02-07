using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore;
using GraphicCustomization;
using static HarmonyLib.Code;

namespace VFEMedieval
{
    public class CompProperties_EditHeraldic : CompProperties
    {
        public CompProperties_EditHeraldic()
        {
            compClass = typeof(CompEditHeraldic);
        }
    }
    public class CompEditHeraldic : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Check if Heraldic Research is completed.
            var research = DefDatabase<ResearchProjectDef>.GetNamed("VFEM2_Heraldry");
            if (research != null && research.IsFinished)
            {
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/CustomizeHeraldry"),
                    defaultLabel = string.Format("VFEM2_StyleHeraldics".Translate(), parent.def.label),
                    action = delegate
                    {
                        var window = new Dialog_Heraldic(parent as Pawn);
                        Find.WindowStack.Add(window);
                    }
                };
            }
        }
    }

    public class Dialog_Heraldic : Window
    {
        private Pawn pawn;
        private static readonly Vector2 ButSize = new Vector2(200f, 40f);
        private List<HeraldicBase> heraldicPatterns;
        private List<HeraldicBase> heraldicSymbols;
        private int currentSymbolIndex = 0;
        private int currentPatternIndex = 0;
        private HeraldicSettings heraldic;

        private List<TabRecord> tabs = new List<TabRecord>();

        private float viewRectHeight;
        private Vector2 apparelColorScrollPosition;

        public override Vector2 InitialSize => new Vector2(900f, 580f);

        private List<Color> allColors;
        private List<Color> AllColors
        {
            get
            {
                if (allColors == null)
                {
                    allColors = new List<Color>
                    {
                        new Color(0.05f, 0.05f, 0.05f),
                        new Color(0.15f, 0.15f, 0.15f),
                        new Color(0.9f, 0.9f, 0.9f),
                        new Color(1, 1, 1),

                        // More Yellows
                        new Color(0.5f, 0.5f, 0.25f),
                        new Color(0.9f, 0.9f, 0.5f),
                        new Color(0.9f, 0.8f, 0.5f),

                        // Reds
                        new Color(0.45f, 0.2f, 0.2f),
                        new Color(0.5f, 0.25f, 0.25f),
                        new Color(0.9f, 0.5f, 0.5f),

                        // Blues
                        new Color(0.15f, 0.28f, 0.43f),
                    };
                    if (ModsConfig.IdeologyActive && pawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        allColors.Add(pawn.Ideo.ApparelColor);
                    }
                    if (ModsConfig.IdeologyActive && pawn.story != null && !pawn.DevelopmentalStage.Baby() && pawn.story.favoriteColor.HasValue && !allColors.Any((Color c) => pawn.story.favoriteColor.Value.IndistinguishableFrom(c)))
                    {
                        allColors.Add(pawn.story.favoriteColor.Value);
                    }
                    foreach (ColorDef colDef in DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Ideo || x.colorType == ColorType.Misc))
                    {
                        if (!allColors.Any((Color x) => x.IndistinguishableFrom(colDef.color)))
                        {
                            allColors.Add(colDef.color);
                        }
                    }
                    allColors.SortByColor((Color x) => x);
                }
                return allColors;
            }
        }

        public Dialog_Heraldic(Pawn pawn)
        {
            this.pawn = pawn;
            forcePause = true;
            closeOnAccept = false;
            closeOnCancel = false;
            heraldic = new HeraldicSettings(pawn.Faction);
            string truePatternName = pawn.GetStringByTag(Heraldic.MASK_PATH)?.value;
            string trueSymbolName = pawn.GetStringByTag(Heraldic.SYMBOL_PATH)?.value;

            heraldicPatterns = DefDatabase<HeraldicPattern>.AllDefsListForReading.Cast<HeraldicBase>().ToList();
            heraldicSymbols = DefDatabase<HeraldicSymbol>.AllDefsListForReading.Cast<HeraldicBase>().ToList();

            currentSymbolIndex = heraldicSymbols.FindIndex(x => x.path == trueSymbolName);
            currentPatternIndex = heraldicPatterns.FindIndex(x => x.path == truePatternName);

            if (currentSymbolIndex == -1)
            {
                currentSymbolIndex = 0;
                Log.Error($"Symbol not found {trueSymbolName} in symbol list. The listed items are:\n" +
                    $"{string.Join("\n", heraldicSymbols.Select(x => $"{x.defName} ({x.path})"))}\n"
                    );
            }
            if (currentPatternIndex == -1)
            {
                currentPatternIndex = 0;
                Log.Error($"Pattern not found {trueSymbolName} in symbol list. The listed items are:\n" +
                    $"{string.Join("\n", heraldicPatterns.Select(x => $"{x.defName} ({x.path})"))}\n"
                    );
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect = new Rect(inRect)
            {
                height = Text.LineHeight * 2f
            };
            Widgets.Label(rect, "VFEM2_StyleHeraldics".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
            Text.Font = GameFont.Small;
            inRect.yMin = rect.yMax + 4f;
            Rect rect2 = inRect;
            rect2.width *= 0.3f;
            rect2.yMax -= ButSize.y + 4f;
            DrawPawn(rect2);
            Rect rect3 = inRect;
            rect3.xMin = rect2.xMax + 10f;
            rect3.yMax -= ButSize.y + 4f;
            DrawMainUI(rect3);
            DrawBottomButtons(inRect);
        }

        private void DrawPawn(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMin = rect.yMax - Text.LineHeight * 2f;
            rect.yMax = rect2.yMin - 4f;
            Widgets.BeginGroup(rect);
            for (int i = 0; i < 3; i++)
            {
                Rect position = new Rect(0f, rect.height / 3f * (float)i, rect.width, rect.height / 3f).ContractedBy(4f);
                RenderTexture image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(2 - i), new Vector3(0f, 0f, 0.15f), 1.1f, supersample: true, compensateForUIScale: true, true, true, stylingStation: true);
                GUI.DrawTexture(position, image);
            }
            Widgets.EndGroup();
        }

        private void DrawBottomButtons(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Close".Translate()))
            {
                Close();
            }
        }

        private void DrawMainUI(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            TabDrawer.DrawTabs(rect, tabs);
            rect = rect.ContractedBy(18f);
            DrawHeraldicSetup(rect);
        }

        private void DrawHeraldicSetup(Rect rect)
        {
            var iconItem = VFEM_DefOf.VFEM2_HeraldicRugGrand;
            Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
            Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
            int num = 0;
            float curY = rect.y;

            foreach ((string type, var heraldicPart) in heraldic.Items)
            {
                if (heraldicPart is TaggedColor tColor)
                {
                    Color color = tColor.value;

                    Rect rect2 = new Rect(rect.x, curY, viewRect.width, 92f);
                    curY += rect2.height + 0f;

                    Widgets.ColorSelector(rect2, ref color, AllColors, out var _, null, 22, 2, ColorSelecterExtraOnGUI);
                    float num2 = rect2.x;
                    if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        rect2 = new Rect(num2, curY, 180f, 24f);
                        if (Widgets.ButtonText(rect2, "SetIdeoColor".Translate()))
                        {
                            color = pawn.Ideo.ApparelColor;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        num2 += 210f;
                    }
                    Pawn_StoryTracker story = pawn.story;
                    if (story != null && story.favoriteColor.HasValue)
                    {
                        rect2 = new Rect(num2, curY, 180f, 24f);
                        if (Widgets.ButtonText(rect2, "SetFavoriteColor".Translate()))
                        {
                            color = pawn.story.favoriteColor.Value;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                    }
                    if (!tColor.value.IndistinguishableFrom(color))
                    {
                        tColor.value = color;
                        RefreshAllGraphics();
                    }
                }
                else if (heraldicPart is TaggedText tText)
                {
                    var targetList = type == Heraldic.MASK_PATH ? heraldicPatterns : heraldicSymbols;
                    int currentIdx = type == Heraldic.MASK_PATH ? currentPatternIndex : currentSymbolIndex;

                    var currentDef = targetList[currentIdx];

                    Rect rect2 = new Rect(rect.x, curY, viewRect.width, 34f);
                    Widgets.Label(rect2, (type == Heraldic.MASK_PATH ?
                          "VFEM2_SetPattern".Translate()
                        : "VFEM2_SetSymbol".Translate()).CapitalizeFirst());

                    curY += rect2.height + 8;
                    Rect rect3 = new Rect(rect.x, curY, viewRect.width, 38f);

                    curY += rect3.height + 0f;
                    MakeFloatOptionButtons(rect3, delegate
                    {
                        currentIdx--;
                    }, delegate
                    {
                        FloatMenuUtility.MakeMenu(targetList, (HeraldicBase entry) => entry.LabelCap, (HeraldicBase variant) => delegate
                        {
                            Log.Message($"Selected {variant.defName} ({variant.path})");
                            currentIdx = targetList.FindIndex(x => x.path == variant.path);
                            if (currentIdx == -1)
                            {
                                Log.Error($"Could not find {variant.defName} ({variant.path}) in {type} list. The listed items are:\n" +
                                $"{string.Join("\n", targetList.Select(x => $"{x.defName} ({x.path})" ))}\n"
                                );
                            }
                            SetHeraldicMask(type, tText, currentIdx, variant.path);
                        });
                    }, currentDef.LabelCap, delegate
                    {
                        currentIdx++;
                    });
                    if (currentIdx < 0) currentIdx = targetList.Count - 1;
                    if (currentIdx >= targetList.Count) currentIdx = 0;

                    currentDef = targetList[currentIdx];
                    if (tText.value != currentDef.path)
                    {
                        SetHeraldicMask(type, tText, currentIdx, currentDef.path);
                    }
                }

                curY += 34f;
            }
            if (Event.current.type == EventType.Layout)
            {
                viewRectHeight = curY - rect.y;
            }
            Widgets.EndScrollView();
        }

        private void SetHeraldicMask(string type, TaggedText tText, int currentIdx, string path)
        {
            if (type == Heraldic.MASK_PATH) currentPatternIndex = currentIdx;
            else currentSymbolIndex = currentIdx;
            tText.value = path;
            RefreshAllGraphics();
        }

        public static void RefreshAllGraphics()
        {
            // Get all things that have a graphic run Notify_ColorChanged on them.
            var allThings = Find.Maps.Select(map => map.listerThings.AllThings).SelectMany(x => x);


            foreach (var thing in allThings)
            {
                thing.Notify_ColorChanged();
            }
            // Get all pawns and dirty graphics on them.
            foreach (var pawn in Find.Maps.Select(map => map.mapPawns.AllPawns).SelectMany(x => x))
            {
                // Notify_ColorChanged on their weapon.
                if (pawn.equipment != null)
                {
                    foreach (var eq in pawn.equipment.equipment)
                    {
                        eq.Notify_ColorChanged();
                    }
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        private void ColorSelecterExtraOnGUI(Color color, Rect boxRect)
        {
            Texture2D texture2D = null;
            TaggedString taggedString = null;
            bool flag = Mouse.IsOver(boxRect);
            if (texture2D != null)
            {
                Rect position = boxRect.ContractedBy(4f);
                GUI.color = Color.black.ToTransparent(0.2f);
                GUI.DrawTexture(new Rect(position.x + 2f, position.y + 2f, position.width, position.height), texture2D);
                GUI.color = Color.white.ToTransparent(0.8f);
                GUI.DrawTexture(position, texture2D);
                GUI.color = Color.white;
            }
            if (!taggedString.NullOrEmpty())
            {
                TooltipHandler.TipRegion(boxRect, taggedString);
            }
        }

        public void MakeFloatOptionButtons(Rect floatMenuButtonsRect, Action leftAction, Action centerAction, string centerButtonName, Action rightAction)
        {
            Widgets.DrawHighlight(floatMenuButtonsRect);
            Rect rect = new Rect(floatMenuButtonsRect.x, floatMenuButtonsRect.y, 32f, 32f);
            Rect rect2 = new Rect(floatMenuButtonsRect.xMax - 32f, floatMenuButtonsRect.y, 32f, 32f);
            Rect rect3 = new Rect(floatMenuButtonsRect.x + 32f, floatMenuButtonsRect.y, floatMenuButtonsRect.width - 64f, 32f);
            if (Dialog_GraphicCustomization.ButtonTextSubtleCentered(rect, "<"))
            {
                leftAction();
            }
            if (Dialog_GraphicCustomization.ButtonTextSubtleCentered(rect3, centerButtonName))
            {
                centerAction();
            }
            if (Dialog_GraphicCustomization.ButtonTextSubtleCentered(rect2, ">"))
            {
                rightAction();
            }
        }
    }

}
