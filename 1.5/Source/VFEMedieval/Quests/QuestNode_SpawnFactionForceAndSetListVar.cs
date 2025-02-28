using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using RimWorld.QuestGen;
using RimWorld;

namespace VFEMedieval
{
    public class QuestNode_SpawnFactionForceAndSetListVar : QuestNode
    {
        public override bool TestRunInt(Slate slate)
        {
            return QuestGen_Get.GetMap() != null && slate.Get<Faction>("askerFaction", null) != null
                && slate.Get<Faction>("siteFaction", null) != null;
        }

        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            Map map = QuestGen_Get.GetMap();

            Faction askerFaction = slate.Get<Faction>("askerFaction");
            Faction siteFaction = slate.Get<Faction>("siteFaction");
            float points = slate.Get("points", 0f);

            List<Pawn> enemyUnitPawns = GeneratePawnList(siteFaction, points, map);
            slate.Set("EnemyUnitPawns", enemyUnitPawns);
            string enemyUnitList = FormatPawnListToString(enemyUnitPawns);
            slate.Set("EnemyUnitList", enemyUnitList);

            List<Pawn> friendlyUnitPawns = GeneratePawnList(askerFaction, points / 2f, map);
            slate.Set("FriendlyUnitPawns", friendlyUnitPawns);
            string friendlyUnitList = FormatPawnListToString(friendlyUnitPawns);
            slate.Set("FriendlyUnitList", friendlyUnitList);
        }

        private List<Pawn> GeneratePawnList(Faction faction, float points, Map map)
        {
            if (points <= 0)
            {
                Log.Warning($"GeneratePawnList: Points are zero or negative ({points}), no pawns spawned for {faction.Name}.");
                return new List<Pawn>();
            }

            PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = faction,
                points = points,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack
            };

            List<Pawn> generatedPawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true).ToList();
            if (!generatedPawns.Any())
            {
                Log.Warning($"GeneratePawnList: No pawns generated for {faction.Name} with {points} points.");
                return new List<Pawn>();
            }

            return generatedPawns;
        }

        private string FormatPawnListToString(List<Pawn> pawns)
        {
            if (pawns == null || !pawns.Any())
            {
                return "";
            }
            return pawns.GroupBy(p => p.kindDef).Select(group => $"{group.Count()} {group.Key.label}").ToCommaList();
        }
    }
}