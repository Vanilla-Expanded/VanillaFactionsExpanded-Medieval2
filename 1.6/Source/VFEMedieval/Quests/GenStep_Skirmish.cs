using System.Collections.Generic;
using RimWorld;
using Verse.AI.Group;
using Verse;
using Verse.AI;
using System.Linq;

namespace VFEMedieval
{
    public class GenStep_Skirmish : GenStep
    {
        public override int SeedPart => 347859235;

        public override void Generate(Map map, GenStepParams parms)
        {
            QuestPart_FactionForces questPart = null;
            foreach (var quest in Find.QuestManager.QuestsListForReading)
            {
                List<QuestPart> questParts = quest.PartsListForReading;
                for (int i = 0; i < questParts.Count; i++)
                {
                    if (questParts[i] is QuestPart_FactionForces factionForcesPart && factionForcesPart.site == parms.sitePart.site)
                    {
                        questPart = factionForcesPart;
                        break;
                    }
                }
            }

            if (questPart is null)
            {
                Log.Error("VFEM_Skirmish quest generated without corresponding quest part.");
                return;
            }

            Faction friendlyFaction = questPart.friendlyFaction;
            Faction enemyFaction = questPart.enemyFaction;
            if (friendlyFaction == null || enemyFaction == null)
            {
                Log.Error("VFEM_Skirmish quest generated without factions.");
                return;
            }
            List<Pawn> friendlyPawns = questPart.friendlyUnitPawns.Select(x => PawnGenerator.GeneratePawn(x, friendlyFaction)).ToList();
            List<Pawn> enemyPawns = questPart.enemyUnitPawns.Select(x => PawnGenerator.GeneratePawn(x, enemyFaction)).ToList();
            questPart.enemyUnitPawnsGenerated = enemyPawns;
            bool vertical = Rand.Bool;
            IntVec3 friendlySpawnSpot = FindEdgeCell(map, vertical ? Rot4.North : Rot4.East);
            IntVec3 enemySpawnSpot = FindEdgeCell(map, vertical ? Rot4.South : Rot4.West);

            if (!friendlySpawnSpot.IsValid || !enemySpawnSpot.IsValid)
            {
                friendlySpawnSpot = CellFinder.RandomEdgeCell(map);
                enemySpawnSpot = CellFinder.RandomEdgeCell(map);
            }

            SpawnPawns(friendlyPawns, friendlySpawnSpot, map);
            SpawnPawns(enemyPawns, enemySpawnSpot, map);
            MapGenerator.PlayerStartSpot = friendlySpawnSpot;
            LordMaker.MakeNewLord(friendlyFaction, new LordJob_AssaultColony(friendlyFaction), map, friendlyPawns);
            LordMaker.MakeNewLord(enemyFaction, new LordJob_AssaultColony(enemyFaction), map, enemyPawns);
            
            foreach (Pawn friendlyPawn in friendlyPawns)
            {
                friendlyPawn.mindState.enemyTarget = enemyPawns.RandomElement();
            }

            foreach (Pawn enemyPawn in enemyPawns)
            {
                enemyPawn.mindState.enemyTarget = friendlyPawns.RandomElement();
            }
        }

        private void SpawnPawns(List<Pawn> pawns, IntVec3 spawnSpot, Map map)
        {
            foreach (var pawn in pawns)
            {
                var walkableCell = CellFinder.RandomClosewalkCellNear(spawnSpot, map, 4, null);
                GenSpawn.Spawn(pawn, walkableCell, map);
            }
        }

        private IntVec3 FindEdgeCell(Map map, Rot4 side)
        {
            if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && !c.Roofed(map),
            map, side, CellFinder.EdgeRoadChance_Hostile, out var result))
            {
                return result;
            }
            var edgeCell = GetEdgeCell(map, side);
            RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile);
            return result;
        }

        private IntVec3 GetEdgeCell(Map map, Rot4 side)
        {
            switch (side.AsInt)
            {
                case 0:
                    return new IntVec3(map.Size.x / 2, 0, 1);
                case 2:
                    return new IntVec3(map.Size.x / 2, 0, map.Size.z - 2);
                case 3:
                    return new IntVec3(1, 0, map.Size.z / 2);
                case 1:
                    return new IntVec3(map.Size.x - 2, 0, map.Size.z / 2);
                default:
                    return IntVec3.Invalid;
            }
        }
    }
}