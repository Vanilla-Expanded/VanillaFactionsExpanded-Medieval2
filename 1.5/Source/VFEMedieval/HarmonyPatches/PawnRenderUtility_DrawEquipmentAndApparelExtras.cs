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


    [HarmonyPatch(typeof(PawnRenderUtility))]
    [HarmonyPatch("DrawEquipmentAndApparelExtras")]
    public static class VFEMedieval_PawnRenderUtility_DrawEquipmentAndApparelExtras_Patch
    {
      
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            if (pawn.jobs?.curDriver is JobDriver_PlayArchery playArchery)
            {
                playArchery.DrawEquipment(drawPos, facing, flags);
                return false;
            }
            if (pawn.jobs?.curDriver is JobDriver_TrainAtDummy trainDummy)
            {
                trainDummy.DrawEquipment(drawPos, facing, flags);
                return false;
            }
            return true;
        }



    
    }








}
