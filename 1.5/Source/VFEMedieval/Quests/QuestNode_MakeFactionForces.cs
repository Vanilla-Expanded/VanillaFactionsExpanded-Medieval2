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
    public class QuestNode_MakeFactionForces : QuestNode_MakeFactionForcesBase
    {
        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            Quest quest = QuestGen.quest;

            Faction askerFaction = slate.Get<Faction>("askerFaction");
            Faction siteFaction = slate.Get<Faction>("siteFaction");

            float points = slate.Get("points", 0f);
            List<Pawn> enemyUnitPawns = GeneratePawnList(siteFaction, points * 1.5f, site);
            string enemyUnitList = FormatPawnListToString(enemyUnitPawns);
            slate.Set("EnemyUnitList", enemyUnitList);

            List<Pawn> friendlyUnitPawns = GeneratePawnList(askerFaction, points / 2f, site);
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
    }
}