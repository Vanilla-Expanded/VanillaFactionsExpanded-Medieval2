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


    [HarmonyPatch(typeof(JobDriver_Lovin))]
    [HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
    public static class VFEMedieval_JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        [HarmonyPostfix]
        public static void ModifyMTB(ref int __result, Pawn pawn)
        {

            if (StaticCollections.pawnMTBLovinMultiplier.ContainsKey(pawn))
            {
                __result = (int)(__result*StaticCollections.pawnMTBLovinMultiplier[pawn]);
            }



        }
    }








}
