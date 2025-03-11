using Verse;
using RimWorld;

namespace VFEMedieval
{
    public static class ResearchPatchesHelper
    {
        public static float GetMaynardCostFactor()
        {
            if (Find.Storyteller?.def == VFEM_DefOf.VFEM_MaynardMedieval)
            {
                int finishedProjectsCount = DefDatabase<ResearchProjectDef>.AllDefsListForReading.FindAll(x => x.IsFinished).Count;
                return 1f + (finishedProjectsCount * 0.01f);
            }
            return 1f;
        }
    }
}