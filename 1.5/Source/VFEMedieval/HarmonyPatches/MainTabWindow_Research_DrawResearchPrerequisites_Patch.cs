using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawResearchPrerequisites")]
    public static class MainTabWindow_Research_DrawResearchPrerequisites_Patch
    {
        public static void Prefix(Rect rect, ref float y, ResearchProjectDef project)
        {
            if (Find.Storyteller?.def == VFEM_DefOf.VFEM_MaynardMedieval)
            {
                Rect rect2 = new Rect(0f, y, rect.width, 0f);
                string text = "VFEM_MaynardMedievalResearchModifier".Translate(project.CostApparent.ToString("F0"));
                Widgets.LabelCacheHeight(ref rect2, text);
                y += rect2.height;
            }
        }
    }
}