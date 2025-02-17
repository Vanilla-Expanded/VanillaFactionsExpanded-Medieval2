using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", new Type[] { typeof(FactionGeneratorParms) })]
    public static class FactionGenerator_NewGeneratedFaction_Patch
    {
        public static void Postfix(FactionGeneratorParms parms, ref Faction __result)
        {
            if (__result != null && __result.def == VFEM_DefOf.VFEM2_MerchantGuild)
            {
                GenerateMerchantCaravans(__result);
            }
        }

        public static void GenerateMerchantCaravans(Faction merchantFaction)
        {
            var spawnCount = Mathf.RoundToInt(VFEMedievalSettings.merchantCaravanSpawnCount 
                * Find.World.info.overallPopulation.GetScaleFactor());
            for (var i = 0; i < spawnCount; i++)
            {
                var merchantGuild = (MerchantGuild)WorldObjectMaker.MakeWorldObject(VFEM_DefOf.VFEM2_MerchantGuildCaravan);
                merchantGuild.Tile = TileFinder.RandomSettlementTileFor(merchantFaction);
                merchantGuild.SetFaction(merchantFaction);
                Find.WorldObjects.Add(merchantGuild);
            }
        }
    }
}