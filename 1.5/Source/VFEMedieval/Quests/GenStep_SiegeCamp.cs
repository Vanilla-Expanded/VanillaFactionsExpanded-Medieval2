using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Linq;
using KCSG;
using RimWorld.BaseGen;

namespace VFEMedieval
{
    public class GenStep_SiegeCamp : GenStep
    {
        public override int SeedPart => 347859236; // Changed SeedPart to avoid conflict

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
            List<Pawn> defenderPawns = questPart.defenderPawns;

            if (defenderPawns == null || !defenderPawns.Any())
            {
                Log.Error("VFEM_SiegeCamp quest generated without defender pawns.");
                return;
            }

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

            // Push additional resolver symbol
            BaseGen.symbolStack.Push("kcsg_runresolvers", resolveParams, null);
            // Start gen
            SettlementGenUtils.Generate(resolveParams, map, GenOption.settlementLayout);
            
            BaseGen.Generate();

            SpawnPawns(defenders, rect, map);
            LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, generationLocation), map, defenders);
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