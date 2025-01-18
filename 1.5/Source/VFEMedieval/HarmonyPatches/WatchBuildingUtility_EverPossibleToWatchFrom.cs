using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using Verse.AI;



namespace VFEMedieval
{
    [HarmonyPatch(typeof(WatchBuildingUtility))]
    [HarmonyPatch("EverPossibleToWatchFrom")]
    public static class VFEMedieval_WatchBuildingUtility_EverPossibleToWatchFrom_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, IntVec3 watchCell, IntVec3 buildingCenter, Map map, bool bedAllowed, ThingDef def)
        {
            if (__result && def == VFEM_DefOf.VFEM2_ArcheryTarget)
            {
                foreach (var cell in GenSight.PointsOnLineOfSight(watchCell, buildingCenter))
                {
                    var plant = cell.GetPlant(map);
                    if (plant != null && plant.def.plant.IsTree)
                    {
                        Log.Message("Cannot watch, tree is obstructing");
                        __result = false;
                    }
                }
            }
        }
    }








}
