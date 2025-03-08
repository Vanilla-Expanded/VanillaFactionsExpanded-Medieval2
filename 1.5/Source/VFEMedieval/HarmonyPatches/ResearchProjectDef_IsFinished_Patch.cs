using HarmonyLib;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ResearchProjectDef), "IsFinished", MethodType.Getter)]
    public static class ResearchProjectDef_IsFinished_Patch
    {
        public static void Prefix()
        {
            ResearchProjectDef_Cost_Patch.allowCheckingCompletedResearch = true;
        }
        public static void Postfix()
        {
            ResearchProjectDef_Cost_Patch.allowCheckingCompletedResearch = false;
        }
    }
}