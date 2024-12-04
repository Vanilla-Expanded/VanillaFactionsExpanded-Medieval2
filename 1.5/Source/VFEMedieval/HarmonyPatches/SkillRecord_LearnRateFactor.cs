using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;



namespace VFEMedieval
{


    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("LearnRateFactor")]
    public static class VFEMedieval_SkillRecord_LearnRateFactor_Patch
    {
        [HarmonyPostfix]
        public static void ModifyLearnRate(SkillRecord __instance, Pawn ___pawn,ref float __result)
        {

            if (StaticCollections.pawnLearningFactorSinglePassionMultiplier.ContainsKey(___pawn))
            {
                if(__instance.passion== Passion.Minor)
                {
                    __result += StaticCollections.pawnLearningFactorSinglePassionMultiplier[___pawn];
                }
                             
            }
            if (StaticCollections.pawnLearningFactorDoublePassionMultiplier.ContainsKey(___pawn))
            {
                if (__instance.passion == Passion.Major)
                {
                    __result += StaticCollections.pawnLearningFactorDoublePassionMultiplier[___pawn];
                }

            }



        }
    }








}
