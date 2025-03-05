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
[HotSwappable]
    public class QuestNode_MakeSiegeCampFactionForces : QuestNode
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

            Faction siteFaction = slate.Get<Faction>("siteFaction");

            float points = slate.Get("points", 0f);
            List<Pawn> defenderPawns = GenerateDefenderPawnList(siteFaction, points / 2f, site); // 50% defender force
            string defenderList = FormatPawnListToString(defenderPawns);
            slate.Set("ListOfEnemies", defenderList);

            List<Pawn> potentialAttackerPawns = GeneratePotentialAttackerPawnList(siteFaction, points * 1.5f, site); // 150% potential attacker force
            string potentialAttackerList = FormatPawnListToString(potentialAttackerPawns);
            slate.Set("ListOfPotentialEnemies", potentialAttackerList);

            QuestPart_SiegeCampFactionForces questPart = new QuestPart_SiegeCampFactionForces();
            questPart.inSignalEnable = quest.AddedSignal;
            questPart.site = slate.Get<Site>("site");
            questPart.siteFaction = siteFaction;
            questPart.defenderPawns = defenderPawns;
            questPart.potentialAttackerPawns = potentialAttackerPawns;
            questPart.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
            quest.AddPart(questPart);
        }

        private List<Pawn> GenerateDefenderPawnList(Faction faction, float points, Site site)
        {
            return GeneratePawnList(faction, points, site);
        }

        private List<Pawn> GeneratePotentialAttackerPawnList(Faction faction, float points, Site site)
        {
            return GeneratePawnList(faction, points, site);
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