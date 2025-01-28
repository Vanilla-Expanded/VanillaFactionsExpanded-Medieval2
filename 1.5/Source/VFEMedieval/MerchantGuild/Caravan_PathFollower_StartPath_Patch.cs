using HarmonyLib;
using RimWorld.Planet;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "StartPath")]
    public static class Caravan_PathFollower_StartPath_Patch
    {
        public static void Postfix(Caravan_PathFollower __instance, int destTile)
        {
            if (Caravan_PathFollower_ExposeData_Patch.merchantsToFollow.TryGetValue(__instance, out var merchant)
                && destTile != merchant.Tile)
            {
                Caravan_PathFollower_ExposeData_Patch.merchantsToFollow.Remove(__instance);
            }
        }
    }
}