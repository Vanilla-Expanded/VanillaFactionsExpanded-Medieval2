using HarmonyLib;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Projectile), "HitFlags", MethodType.Getter)]
    public static class Projectile_HitFlags_Patch
    {
        public static void Postfix(Projectile __instance, ref ProjectileHitFlags __result)
        {
            if (__instance is WarbowArrow && __instance.ticksToImpact <= 10)
            {
                __result = ProjectileHitFlags.All;
            }
        }
    }

}
