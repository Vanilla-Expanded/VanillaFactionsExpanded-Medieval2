using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(FactionUIUtility), "DoWindowContents")]
    public static class FactionUIUtility_DoWindowContents_Patch
    {
        public static void Prefix()
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_MerchantGuild);
            if (faction != null)
            {
                faction.hidden = false;
            }
        }

        public static void Postfix()
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_MerchantGuild);
            if (faction != null)
            {
                faction.hidden = true;
            }
        }
    }
}