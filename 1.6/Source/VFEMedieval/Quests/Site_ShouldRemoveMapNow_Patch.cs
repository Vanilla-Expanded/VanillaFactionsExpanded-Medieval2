using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using RimWorld.Planet;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(Site), "ShouldRemoveMapNow")]
    public static class Site_ShouldRemoveMapNow_Patch
    {
        public static void Postfix(Site __instance, ref bool __result, ref bool alsoRemoveWorldObject)
        {
            if (__result && __instance.parts != null)
            {
                foreach (var part in __instance.parts)
                {
                    if (part.def.Worker is SitePartWorker_Siege)
                    {
                        foreach (var quest in Find.QuestManager.activeQuests)
                        {
                            List<QuestPart> questParts = quest.PartsListForReading;
                            for (int i = 0; i < questParts.Count; i++)
                            {
                                if (questParts[i] is QuestPart_SiegeCampFactionForces siegeCampFactionForcesPart
                                    && siegeCampFactionForcesPart.site == __instance)
                                {
                                    alsoRemoveWorldObject = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}