using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VFEMedieval
{
    public class ToolExtended : Tool
    {
        public FloatRange powerMultiplierRange;
    }

    [HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Verb), typeof(Pawn) })]
    public static class VerbProperties_AdjustedMeleeDamageAmount_Patch
    {
        private static void Postfix(ref float __result, Verb ownerVerb, Pawn attacker)
        {
            if (ownerVerb.tool is ToolExtended toolExtended)
            {
                __result *= toolExtended.powerMultiplierRange.RandomInRange;
            }
        }
    }
}
