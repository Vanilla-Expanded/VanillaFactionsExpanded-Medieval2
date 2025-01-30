using HarmonyLib;
using RimWorld.Planet;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "PatherTick")]
    public static class Caravan_PathFollower_PatherTick_Patch
    {
        public static void Prefix(Caravan_PathFollower __instance)
        {
            if (Caravan_PathFollower_ExposeData_Patch.merchantsToFollow.TryGetValue(__instance, out var merchant)
                && __instance.Destination != merchant.Tile)
            {
                __instance.StartPath(merchant.Tile, new CaravanArrivalAction_Barter(merchant), repathImmediately: true);
            }
        }
    }
}