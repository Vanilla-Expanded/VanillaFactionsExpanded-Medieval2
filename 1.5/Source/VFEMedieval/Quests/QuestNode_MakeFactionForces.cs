using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using RimWorld.QuestGen;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace VFEMedieval
{
    public class QuestNode_MakeFactionForces : QuestNode
    {
        public override bool TestRunInt(Slate slate)
        {
            return true;
        }

        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            Quest quest = QuestGen.quest;

            Faction askerFaction = slate.Get<Faction>("askerFaction");
            Faction siteFaction = slate.Get<Faction>("siteFaction");

            float points = slate.Get("points", 0f);
            Log.Message("points?: " + points);
            List<Pawn> enemyUnitPawns = GeneratePawnList(siteFaction, points * 1.5f, site);
            slate.Set("EnemyUnitPawns", enemyUnitPawns);
            string enemyUnitList = FormatPawnListToString(enemyUnitPawns);
            slate.Set("EnemyUnitList", enemyUnitList);

            List<Pawn> friendlyUnitPawns = GeneratePawnList(askerFaction, points / 2f, site);
            slate.Set("FriendlyUnitPawns", friendlyUnitPawns);
            string friendlyUnitList = FormatPawnListToString(friendlyUnitPawns);
            slate.Set("FriendlyUnitList", friendlyUnitList);

            QuestPart_FactionForces questPart = new QuestPart_FactionForces();
            questPart.inSignalEnable = quest.AddedSignal;
            questPart.site = slate.Get<Site>("site");
            questPart.enemyFaction = siteFaction;
            questPart.friendlyFaction = askerFaction;
            questPart.enemyUnitPawns = enemyUnitPawns;
            questPart.friendlyUnitPawns = friendlyUnitPawns;
            questPart.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
            quest.AddPart(questPart);
        }

        private List<Pawn> GeneratePawnList(Faction faction, float points, Site site)
        {
            PawnGroupMakerParms pawnGroupMakerParms = GetParms(faction, points, site);
            points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms));
            List<Pawn> generatedPawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true).ToList();
            if (!generatedPawns.Any())
            {
                Log.Warning($"GeneratePawnList: No pawns generated for {faction.Name} with {points} points.");
                return new List<Pawn>();
            }

            return generatedPawns;
        }

        private static PawnGroupMakerParms GetParms(Faction faction, float points, Site site)
        {
            return new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = site.Tile,
                faction = faction,
                points = points,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack
            };
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