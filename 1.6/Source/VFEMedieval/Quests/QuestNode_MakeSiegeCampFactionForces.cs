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
    public class QuestNode_MakeSiegeCampFactionForces : QuestNode_MakeFactionForcesBase
    {
        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            Quest quest = QuestGen.quest;

            Faction siteFaction = slate.Get<Faction>("siteFaction");

            float points = slate.Get("points", 0f);
            List<PawnKindDef> defenderPawns = GeneratePawnKindList(siteFaction, points / 2f, site);
            string defenderList = FormatPawnListToString(defenderPawns);
            slate.Set("ListOfEnemies", defenderList);

            QuestPart_SiegeCampFactionForces questPart = new QuestPart_SiegeCampFactionForces();
            questPart.inSignalEnable = quest.AddedSignal;
            questPart.site = slate.Get<Site>("site");
            questPart.siteFaction = siteFaction;
            questPart.points = points;
            questPart.defenderPawns = defenderPawns;
            questPart.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
            quest.AddPart(questPart);
        }
    }
}
