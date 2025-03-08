using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public static class ResearchUtility
    {
        public static bool recursionTrap;
        public static Dictionary<ResearchManager, Dictionary<ResearchProjectDef, float>> originBaseCostsPerManager = new();
        public static Dictionary<ResearchManager, Dictionary<ResearchProjectDef, float>> cachedBaseCostsPerManager = new();
        public static void TryChangeBaseCost(ResearchProjectDef proj, bool allowCheckingCompletedResearch = false)
        {
            if (recursionTrap) return;
            recursionTrap = true;
            TryChangeBaseCostInternal(proj);
            recursionTrap = false;
        }

        private static void TryChangeBaseCostInternal(ResearchProjectDef proj)
        {
            var manager = Find.ResearchManager;
            if (manager is null) return;
            if (!originBaseCostsPerManager.TryGetValue(manager, out var originBaseCosts))
            {
                originBaseCosts = originBaseCostsPerManager[manager] = new();
            }

            if (!cachedBaseCostsPerManager.TryGetValue(manager, out var cachedBaseCosts))
            {
                cachedBaseCosts = cachedBaseCostsPerManager[manager] = new();
            }

            if (Find.Storyteller?.def == VFEM_DefOf.VFEM_MaynardMedieval)
            {
                var researchProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
                if (originBaseCosts is null || originBaseCosts.Count <= 0)
                {
                    originBaseCosts = new();
                    cachedBaseCosts = new();
                    foreach (var def in researchProjects)
                    {
                        Log.Message(string.Format("VFEM: Processing research project: {0}", def.defName));
                        originBaseCosts[def] = def.baseCost;
                        if (ResearchManager_ExposeData_Patch.finishedCosts.ContainsKey(def) is false
                             && Find.ResearchManager.currentProj != def && def.IsFinished)
                        {
                            ResearchManager_ExposeData_Patch.finishedCosts[def] = def.baseCost;
                            Log.Message(string.Format("VFEM: finishedCosts[{0}] = {1}", def.defName, def.baseCost));
                        }
                    }
                    originBaseCostsPerManager[manager] = originBaseCosts;
                    cachedBaseCostsPerManager[manager] = cachedBaseCosts;
                }

                if (cachedBaseCosts.TryGetValue(proj, out var cachedBaseCost))
                {
                    if (proj.baseCost != cachedBaseCost)
                    {
                        proj.baseCost = cachedBaseCost;
                    }
                }
                else
                {
                    foreach (var item in originBaseCosts.OrderBy(x => x.Key.index).ToList())
                    {
                        var finishedResearches = researchProjects.Where(x => ResearchManager_ExposeData_Patch.finishedCosts.ContainsKey(x)).Count();
                        SetBaseCost(item.Key, finishedResearches, originBaseCosts, cachedBaseCosts);
                    }
                }
            }

            else if (originBaseCosts.NullOrEmpty() is false)
            {
                foreach (var item in originBaseCosts)
                {
                    item.Key.baseCost = item.Value;
                }
            }
            Log.ResetMessageCount();
            return;
        }

        private static void SetBaseCost(ResearchProjectDef proj, int allFinishedProjects, 
        Dictionary<ResearchProjectDef, float> originBaseCosts, Dictionary<ResearchProjectDef, float> cachedBaseCosts)
        {
            if (originBaseCosts.TryGetValue(proj, out var baseCost))
            {
                float researchCostModifier = 1f + (allFinishedProjects * 0.01f);
                baseCost *= researchCostModifier;
                proj.baseCost = Mathf.Round(baseCost);
                cachedBaseCosts[proj] = proj.baseCost;
                Log.Message(string.Format("VFEM: cachedBaseCosts[{0}] = {1}", proj.defName, proj.baseCost));
            }
        }
    }
}