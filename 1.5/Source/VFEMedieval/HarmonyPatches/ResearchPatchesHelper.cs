using Verse;
using RimWorld;

namespace VFEMedieval
{
    public static class ResearchPatchesHelper
    {
        private static int lastCalculatedTick = -1;
        private static float cachedFactor = 1f;
        private const int CacheInvalidationInterval = 30;
        public static float GetMaynardCostFactor()
        {
            if (Find.Storyteller?.def != VFEM_DefOf.VFEM_MaynardMedieval)
            {
                return 1f;
            }
            
            int currentTick = GenTicks.TicksGame;
            bool cacheExpired = lastCalculatedTick < 0 || currentTick >= lastCalculatedTick + CacheInvalidationInterval;
            
            if (cacheExpired)
            {
                lastCalculatedTick = currentTick;
                int finishedProjectsCount = DefDatabase<ResearchProjectDef>
                                                .AllDefsListForReading
                                                .Count(x => x.IsFinished);
                cachedFactor = 1f + (finishedProjectsCount * 0.01f);
            }
            return cachedFactor;
        }
    }
}