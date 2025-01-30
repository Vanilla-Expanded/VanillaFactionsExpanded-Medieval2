using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "ExposeData")]
    public static class Caravan_PathFollower_ExposeData_Patch
    {
        public static Dictionary<Caravan_PathFollower, MerchantGuild> merchantsToFollow = new Dictionary<Caravan_PathFollower, MerchantGuild>();
        public static void Postfix(Caravan_PathFollower __instance)
        {
            merchantsToFollow.TryGetValue(__instance, out var merchant);
            Scribe_References.Look(ref merchant, "merchantGuild");
            if (merchant != null)
            {
                merchantsToFollow[__instance] = merchant;
            }
        }
    }
}