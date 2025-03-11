using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchProjectDef), "ProgressApparent", MethodType.Getter)]
    public static class ResearchProjectDef_ProgressApparent_Patch
    {
        public static void Postfix(ref float __result)
        {
            __result = Mathf.Round(__result * ResearchPatchesHelper.GetMaynardCostFactor());
        }
    }
}