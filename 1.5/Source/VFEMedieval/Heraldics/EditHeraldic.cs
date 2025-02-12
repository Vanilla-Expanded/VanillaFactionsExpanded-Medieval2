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
using System.IO;
using System.Diagnostics;

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
            var research = VFEM_DefOf.VFEM2_Heraldry;
            if (research.IsFinished && (parent is Pawn == false || WorldComponent_HeraldryPawns.Instance.heraldry_pawns.Contains(parent)))
            {
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/CustomizeHeraldry"),
                    defaultLabel = string.Format("VFEM2_StyleHeraldics".Translate(), parent.def.label),
                    action = delegate
                    {
                        var window = new Dialog_Heraldic(parent);
                        Find.WindowStack.Add(window);
                    }
                };
            }
        }
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
         
            WorldComponent_HeraldryPawns.Instance.AddHeraldryPawn(pawn);
        }

     
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            bool flag = false;
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                if (wornApparel[i].def?.HasComp<CompEditHeraldic>()==true)
                {
                    flag = true;
                }
               
            }
            List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].def?.HasComp<CompEditHeraldic>() == true)
                {
                    flag = true;
                }

            }
            if (!flag)
            {
                WorldComponent_HeraldryPawns.Instance.RemoveHeraldryPawn(pawn);
            }

            
        }
    }

    public class Dialog_Heraldic : Window
    {
        private enum Mode
        {
            VFEM2_EditThingHeraldry,
            VFEM2_EditFactionHeraldry,
        }

        
        private readonly ILoadReferenceable target;
        private static readonly Vector2 ButSize = new(200f, 40f);
        private List<HeraldicBase> heraldicPatterns;
        private List<HeraldicBase> heraldicSymbols;
        private int currentSymbolIndex = 0;
        private int currentPatternIndex = 0;
        private HeraldicSettings sharedHeraldry;
        private HeraldicSettings individualHeraldry;

        private Mode heraldicEditMode;
        private bool noFaction;

        private float viewRectHeight;
        private Vector2 apparelColorScrollPosition;

        public override Vector2 InitialSize => new(470f, 780f);

        private Pawn Pawn => target as Pawn;

        public ILoadReferenceable ActiveTarget =>
            heraldicEditMode == Mode.VFEM2_EditFactionHeraldry && target is Thing t ? t.Faction : target;
        private List<Color> allColors;
        private List<Color> AllColors
        {
            get
            {
                if (allColors == null)
                {
                    allColors =
                    [
                        new(0.08f, 0.8f, 0.08f),
                        new(0.15f, 0.15f, 0.15f),
                        new(0.9f, 0.9f, 0.9f),
                        Color.white,

                        // More Yellows
                        new(0.5f, 0.5f, 0.25f),
                        new(0.9f, 0.9f, 0.5f),
                        new(0.9f, 0.8f, 0.5f),

                        // Reds
                        new(0.45f, 0.2f, 0.2f),
                        new(0.5f, 0.25f, 0.25f),
                        new(0.9f, 0.5f, 0.5f),

                        // Blues
                        new(0.15f, 0.28f, 0.43f),

                        // Some Pastel Colors
                        new(0.98f, 0.92f, 0.84f),
                        new(0.87f, 0.96f, 0.91f),
                        new(0.94f, 0.87f, 0.96f),
                        new(0.96f, 0.87f, 0.87f),
                        new(0.87f, 0.94f, 0.96f),
                    ];
                    if (ModsConfig.IdeologyActive && Pawn?.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        allColors.Add(Pawn.Ideo.ApparelColor);
                    }
                    if (ModsConfig.IdeologyActive && Pawn?.story != null && !Pawn.DevelopmentalStage.Baby() && Pawn.story.favoriteColor.HasValue && !allColors.Any((Color c) => Pawn.story.favoriteColor.Value.IndistinguishableFrom(c)))
                    {
                        allColors.Add(Pawn.story.favoriteColor.Value);
                    }
                    foreach (ColorDef colDef in DefDatabase<ColorDef>.AllDefs
                        .Where((ColorDef x) => x.colorType == ColorType.Ideo || x.colorType == ColorType.Misc))
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


        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = windowRect.x - InitialSize.x;
        }

        public Dialog_Heraldic(ILoadReferenceable target)
        {
            heraldicEditMode = Mode.VFEM2_EditThingHeraldry;
            noFaction = false;
            this.target = target;
            Faction faction;
            if (target is Thing thing)
            {
                faction = thing.Faction;
                individualHeraldry = new HeraldicSettings(thing);
                if (faction == null)
                {
                    faction = Faction.OfPlayer;
                    noFaction = true;
                    if (!individualHeraldry.HasAllTags())
                    {
                        sharedHeraldry = new HeraldicSettings(faction);
                        sharedHeraldry.SaveTagsTo(thing);  // Save the player tags to it so it has something to load.
                    }
                }
                else
                {
                    sharedHeraldry = new HeraldicSettings(faction);
                }
                individualHeraldry = new HeraldicSettings(thing);
            }
            else if (target is Faction factionTarget)
            {
                faction = factionTarget;
                sharedHeraldry = new HeraldicSettings(faction);
                heraldicEditMode = Mode.VFEM2_EditFactionHeraldry;
            }
            else
            {
                throw new NotImplementedException($"{target.GetType()} is not supported.");
            }

            forcePause = false;
            closeOnAccept = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            preventCameraMotion = false;
            GetPathIndices(target);
        }

        private void GetPathIndices(ILoadReferenceable target)
        {
            heraldicPatterns = DefDatabase<HeraldicPattern>.AllDefsListForReading.Cast<HeraldicBase>().ToList();
            heraldicSymbols = DefDatabase<HeraldicSymbol>.AllDefsListForReading.Cast<HeraldicBase>().ToList();
            currentSymbolIndex = heraldicSymbols.FindIndex(x => x.path == target.GetStringByTag(Heraldic.SYMBOL_PATH)?.value);
            currentPatternIndex = heraldicPatterns.FindIndex(x => x.path == target.GetStringByTag(Heraldic.MASK_PATH)?.value);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect = new(inRect)
            {
                height = Text.LineHeight * 2f
            };
            string title = "";
            if (target is Pawn p) title = p.Name.ToStringShort;
            else if (target is Thing t) title = t.LabelCap;
            else if (target is Faction f) title = f.Name.ToString();
            Widgets.Label(rect, "VFEM2_StyleHeraldics".Translate().CapitalizeFirst() + ": " + title);
            Text.Font = GameFont.Small;
            inRect.yMin = rect.yMax + 4f;

            // Draw top buttons
            DrawTopButtons(inRect);
            inRect.yMin += ButSize.y + 10f;

            Rect rect3 = inRect;
            rect3.xMin = 10f;
            rect3.yMax -= ButSize.y * 3 + 4f;
            DrawMainUI(rect3);
            DrawBottomButtons(inRect);
        }
        private void DrawTopButtons(Rect inRect)
        {
            if (noFaction) return;
            var modePickRect = new Rect(inRect.x + 10f, inRect.y, inRect.width - 20f, ButSize.y);
            if (target is not Faction && Widgets.ButtonText(modePickRect, heraldicEditMode.ToString().Translate()))
            {
                var names = Enum.GetNames(typeof(Mode)).ToList();
                //if (noFaction) names.Remove(Mode.VFEM2_EditFactionHeraldry.ToString());
                FloatMenuUtility.MakeMenu(names, (string entry) => entry.Translate(), (string variant) => delegate
                {
                    heraldicEditMode = (Mode)Enum.Parse(typeof(Mode), variant);
                    GetPathIndices(ActiveTarget);
                });
            }
        }

        private void DrawBottomButtons(Rect inRect)
        {
            float buttonHeight = ButSize.y;
            float buttonWidth = inRect.width - 20f; 

            float yOffset = inRect.yMax - buttonHeight*2 - 30f;

            bool resetActive = heraldicEditMode != Mode.VFEM2_EditFactionHeraldry && !noFaction;
            if (resetActive)
            {
                if (Widgets.ButtonText(new Rect(inRect.x + 10f, yOffset, buttonWidth, buttonHeight), "VFEM2_ResetToFaction".Translate(), active: resetActive))
                {
                    target.RemoveColorTag(Heraldic.MASK_CLR_A);
                    target.RemoveColorTag(Heraldic.MASK_CLR_B);
                    target.RemoveStringTag(Heraldic.MASK_PATH);
                    target.RemoveColorTag(Heraldic.SYMBOL_CLR);
                    target.RemoveStringTag(Heraldic.SYMBOL_PATH);
                    GetPathIndices(ActiveTarget);
                    RefreshAllGraphics();
                    Close(); return;
                }
            }
            yOffset += buttonHeight + 10f;

            if (Widgets.ButtonText(new Rect(inRect.x + 10f, yOffset, buttonWidth, buttonHeight), "Close".Translate()))
            {
                Close();
            }
        }

        private void DrawMainUI(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(18f);
            DrawHeraldicSetup(rect);
        }

        private void DrawHeraldicSetup(Rect rect)
        {
            var iconItem = VFEM_DefOf.VFEM2_HeraldicRugGrand;
            Rect viewRect = new(rect.x, rect.y, rect.width - 34f, viewRectHeight);
            Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
            float curY = rect.y;
            HeraldicSettings activeHeraldry;
            if (heraldicEditMode == Mode.VFEM2_EditFactionHeraldry && target is Thing t)
            {
                activeHeraldry = sharedHeraldry;
            }
            else
            {
                activeHeraldry = individualHeraldry;
            }

            foreach ((string type, var heraldicPart) in activeHeraldry.Items)
            {
                if (heraldicPart is TaggedColor tColor)
                {
                    Color color = tColor.value;

                    Rect colorRect = new(rect.x, curY, viewRect.width, 140);
                    curY += colorRect.height + 0f;

                    Widgets.ColorSelector(colorRect, ref color, AllColors, out var _, null, 22, 2, ColorSelecterExtraOnGUI);
                    float num2 = colorRect.x;
                    if (Pawn?.Ideo is Ideo pawnIdeo && !Find.IdeoManager.classicMode)
                    {
                        colorRect = new Rect(num2, curY, 160f, 24f);
                        if (Widgets.ButtonText(colorRect, "SetIdeoColor".Translate()))
                        {
                            color = pawnIdeo.ApparelColor;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        num2 += 210f;
                    }
                    if (Pawn?.story is Pawn_StoryTracker story && story.favoriteColor.HasValue)
                    {
                        colorRect = new Rect(num2, curY, 160f, 24f);
                        if (Widgets.ButtonText(colorRect, "SetFavoriteColor".Translate()))
                        {
                            color = Pawn.story.favoriteColor.Value;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                    }
                    if (!tColor.value.IndistinguishableFrom(color))
                    {
                        SetColorTag(ActiveTarget, type, tColor, color);
                    }
                }
                else if (heraldicPart is TaggedText tText)
                {
                    var targetList = type == Heraldic.MASK_PATH ? heraldicPatterns : heraldicSymbols;
                    int currentIdx = type == Heraldic.MASK_PATH ? currentPatternIndex : currentSymbolIndex;

                    var currentDef = targetList[currentIdx];

                    Rect titleRect = new(rect.x, curY, viewRect.width, 34f);
                    Text.Font = GameFont.Medium;
                    Widgets.Label(titleRect, (type == Heraldic.MASK_PATH ?
                          "VFEM2_SetPattern".Translate()
                        : "VFEM2_SetSymbol".Translate()).CapitalizeFirst());
                    Text.Font = GameFont.Small;

                    curY += titleRect.height + 8;
                    Rect symbolRect = new(rect.x, curY, viewRect.width, 38f);

                    curY += symbolRect.height + 0f;
                    MakeFloatOptionButtons(symbolRect, delegate
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
                                $"{string.Join("\n", targetList.Select(x => $"{x.defName} ({x.path})"))}\n"
                                );
                            }
                            SetHeraldicMask(ActiveTarget, type, tText, currentIdx, variant.path);
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
                        SetHeraldicMask(ActiveTarget, type, tText, currentIdx, currentDef.path);
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

        private void SetColorTag(ILoadReferenceable activeTarget, string type, TaggedColor tColor, Color color)
        {
            activeTarget.SetColorTag(tColor.tag, color);
            if (heraldicEditMode == Mode.VFEM2_EditFactionHeraldry)
            {
                sharedHeraldry.Refresh();
            }
            else
            {
                individualHeraldry.Refresh();
            }
            RefreshAllGraphics();
        }

        private void SetHeraldicMask(ILoadReferenceable activeTarget, string type, TaggedText tText, int currentIdx, string path)
        {
            if (type == Heraldic.MASK_PATH) currentPatternIndex = currentIdx;
            else currentSymbolIndex = currentIdx;
            activeTarget.SetStringTag(tText.tag, path);
            if (heraldicEditMode == Mode.VFEM2_EditFactionHeraldry)
            {
                sharedHeraldry.Refresh();
            }
            else
            {
                individualHeraldry.Refresh();
            }
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
            Rect rect = new(floatMenuButtonsRect.x, floatMenuButtonsRect.y, 32f, 32f);
            Rect rect2 = new(floatMenuButtonsRect.xMax - 32f, floatMenuButtonsRect.y, 32f, 32f);
            Rect rect3 = new(floatMenuButtonsRect.x + 32f, floatMenuButtonsRect.y, floatMenuButtonsRect.width - 64f, 32f);
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
