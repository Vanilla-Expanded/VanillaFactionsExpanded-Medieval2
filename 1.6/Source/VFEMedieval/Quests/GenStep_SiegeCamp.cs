using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Linq;
using KCSG;
using RimWorld.BaseGen;
using UnityEngine;

namespace VFEMedieval
{

    public class GenStep_SiegeCamp : GenStep
    {
        public override int SeedPart => 347859236;
        public override void Generate(Map map, GenStepParams parms)
        {
            QuestPart_SiegeCampFactionForces questPart = null;
            foreach (var quest in Find.QuestManager.QuestsListForReading)
            {
                List<QuestPart> questParts = quest.PartsListForReading;
                for (int i = 0; i < questParts.Count; i++)
                {
                    if (questParts[i] is QuestPart_SiegeCampFactionForces siegeCampFactionForcesPart && siegeCampFactionForcesPart.site == parms.sitePart.site)
                    {
                        questPart = siegeCampFactionForcesPart;
                        break;
                    }
                }
            }

            if (questPart is null)
            {
                Log.Error("VFEM_SiegeCamp quest generated without corresponding quest part.");
                return;
            }

            Faction siteFaction = questPart.siteFaction;

            if (siteFaction == null)
            {
                Log.Error("VFEM_SiegeCamp quest generated without site faction.");
                return;
            }
            List<Pawn> defenderPawns = questPart.defenderPawns.Select(x => PawnGenerator.GeneratePawn(x, siteFaction)).ToList();
            if (defenderPawns.NullOrEmpty())
            {
                Log.Error("VFEM_SiegeCamp quest generated without defender pawns.");
                return;
            }
            questPart.defenderPawnsGenerated = defenderPawns;
            
            // Spawn tents using CustomGenOption
            var genOption = parms.sitePart.def.GetModExtension<CustomGenOption>();

            IntVec3 generationLocation = map.Center;
            if (generationLocation.Fogged(map) && CellFinder.TryFindRandomSpawnCellForPawnNear(generationLocation, map, out var result,
                extraValidator: (IntVec3 x) => x.Fogged(map) is false))
            {
                generationLocation = result;
            }

            GenerateTents(map, genOption, generationLocation, siteFaction, defenderPawns);
        }

        private void GenerateTents(Map map, CustomGenOption genOption, IntVec3 generationLocation, Faction faction, List<Pawn> defenders)
        {
            GenOption.customGenExt = genOption;
            GenOption.settlementLayout = genOption.chooseFromSettlements.RandomElement();

            int width = GenOption.settlementLayout.settlementSize.x;
            int height = GenOption.settlementLayout.settlementSize.z;

            CellRect rect = CellRect.CenteredOn(generationLocation, width, height);
            rect.ClipInsideMap(map);
            GenOption.GetAllMineableIn(rect, map);

            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.faction = faction;
            resolveParams.rect = rect;
            BaseGen.globalSettings.map = map;

            BaseGen.symbolStack.Push("kcsg_runresolvers", resolveParams, null);
            SettlementGenUtils.Generate(resolveParams, map, GenOption.settlementLayout);

            BaseGen.Generate();
            var pawnSpawnRect = CellRect.CenteredOn(generationLocation, width / 2, height / 2);
            pawnSpawnRect.ClipInsideMap(map);
            SpawnPawns(defenders, pawnSpawnRect, map);
            LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, generationLocation, 180000), map, defenders);
        }

        private void SpawnPawns(List<Pawn> pawns, CellRect cellRect, Map map)
        {
            foreach (var pawn in pawns)
            {
                var walkableCell = CellFinder.RandomClosewalkCellNear(cellRect.RandomCell, map, 4, null);
                GenSpawn.Spawn(pawn, walkableCell, map);
            }
        }
    }
}