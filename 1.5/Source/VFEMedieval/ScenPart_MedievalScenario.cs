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
    public class ScenPart_MedievalScenario : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            Faction enemyFaction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_KingdomRough) ?? Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);
            SettlementLayoutDef settlementLayout = VFEM_DefOf.VFEM2_KeepHousesSettlement;
            GenOption.customGenExt = this.def.GetModExtension<CustomGenOption>();
            GenOption.settlementLayout = settlementLayout;
            IntVec3 generationLocation = map.Center;
            CellRect rect = CellRect.CenteredOn(generationLocation, settlementLayout.settlementSize.x, settlementLayout.settlementSize.z);
            rect.ClipInsideMap(map);
            GenOption.GetAllMineableIn(rect, map);

            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.faction = enemyFaction;
            resolveParams.rect = rect;
            BaseGen.globalSettings.map = map;

            BaseGen.symbolStack.Push("kcsg_runresolvers", resolveParams, null);
            SettlementGenUtils.Generate(resolveParams, map, settlementLayout);
            BaseGen.Generate();

            PawnKindDef lordKind = VFEM_DefOf.VFEM2_Lord;
            PawnKindDef knightKind = VFEM_DefOf.VFEM2_Knight;
            PawnKindDef militiaKind = VFEM_DefOf.VFEM2_Militia;

            IntVec3 castleCenter = generationLocation;

            List<Pawn> enemyPawns = new List<Pawn>();
            Pawn lord = PawnGenerator.GeneratePawn(lordKind, enemyFaction);
            Pawn knight = PawnGenerator.GeneratePawn(knightKind, enemyFaction);
            Pawn militia1 = PawnGenerator.GeneratePawn(militiaKind, enemyFaction);
            Pawn militia2 = PawnGenerator.GeneratePawn(militiaKind, enemyFaction);

            enemyPawns.Add(lord);
            enemyPawns.Add(knight);
            enemyPawns.Add(militia1);
            enemyPawns.Add(militia2);

            GenSpawn.Spawn(lord, CellFinder.RandomClosewalkCellNear(castleCenter, map, 4, null), map);
            GenSpawn.Spawn(knight, CellFinder.RandomClosewalkCellNear(castleCenter, map, 4, null), map);
            GenSpawn.Spawn(militia1, CellFinder.RandomClosewalkCellNear(castleCenter, map, 4, null), map);
            GenSpawn.Spawn(militia2, CellFinder.RandomClosewalkCellNear(castleCenter, map, 4, null), map);

            LordMaker.MakeNewLord(enemyFaction, new LordJob_DefendBase(enemyFaction, castleCenter), map, enemyPawns);

            foreach (var building in map.listerThings.AllThings.Where(x => x is Building && x.def.IsBuildingArtificial).ToList())
            {
                if (Rand.Chance(0.3f))
                {
                    building.HitPoints = Mathf.RoundToInt(building.MaxHitPoints * Rand.Range(0.2f, 0.8f));
                }
                if (Rand.Chance(0.05f))
                {
                    building.Destroy();
                }
            }

            List<List<Thing>> list = new List<List<Thing>>();
            List<Thing> list2 = new List<Thing>();
            foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
            {
                List<Thing> list3 = new List<Thing>();
                list3.Add(startingAndOptionalPawn);
                list.Add(list3);
                foreach (ThingDefCount item in Find.GameInitData.startingPossessions[startingAndOptionalPawn])
                {
                    var possesion = StartingPawnUtility.GenerateStartingPossession(item);
                    startingAndOptionalPawn.inventory.TryAddItemNotForSale(possesion);
                }
            }
            foreach (ScenPart allPart in Find.Scenario.AllParts)
            {
                list2.AddRange(allPart.PlayerStartingThings());
            }
            int num = 0;
            foreach (Thing item2 in list2)
            {
                if (item2.def.CanHaveFaction)
                {
                    item2.SetFactionDirect(Faction.OfPlayer);
                }
                list[num].Add(item2);
                num++;
                if (num >= list.Count)
                {
                    num = 0;
                }
            }

            float minRadius = 50f;
            float maxRadius = 70f;

            foreach (var item in list.SelectMany((List<Thing> x) => x).ToList())
            {
                IntVec3 spawnPos;
                if (CellFinder.TryFindRandomCellNear(castleCenter, map, Mathf.RoundToInt(maxRadius),
                    (IntVec3 cell) => cell.DistanceTo(castleCenter) >= minRadius && cell.DistanceTo(castleCenter) <= maxRadius
                     && cell.Standable(map), out spawnPos))
                {
                    GenSpawn.Spawn(item, spawnPos, map);
                }
                else
                {
                    spawnPos = CellFinderLoose.RandomCellWith((IntVec3 cell) => cell.Standable(map), map);
                    GenSpawn.Spawn(item, spawnPos, map);
                }
            }
            FloodFillerFog.DebugRefogMap(map);  
            
            foreach (var item in map.listerThings.AllThings.Where(x => x.def.category == ThingCategory.Item).ToList())
            {
                item.SetForbidden(true, false);
            }
        }
    }
}