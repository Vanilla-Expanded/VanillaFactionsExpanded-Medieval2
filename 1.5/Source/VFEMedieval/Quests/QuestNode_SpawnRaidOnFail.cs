using System.Collections.Generic;
using Verse;
using RimWorld.QuestGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEMedieval
{
    public class QuestNode_SpawnRaidOnFail : QuestNode_MakeFactionForcesBase
    {
        public override bool TestRunInt(Slate slate)
        {
            return true;
        }

        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            Faction siteFaction = slate.Get<Faction>("siteFaction");
            float points = slate.Get("points", 0f);
            Map map = slate.Get<Map>("map");

            List<Pawn> raiders = GeneratePawnList(siteFaction, points * 1.5f, site);
            string potentialAttackerList = FormatPawnListToString(raiders);
            slate.Set("ListOfPotentialEnemies", potentialAttackerList);

            QuestPart_SpawnRaidOnFail questPart = new QuestPart_SpawnRaidOnFail();
            questPart.faction = siteFaction;
            questPart.pawns = raiders;
            questPart.map = map;
            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(slate.Get<string>("inSignal"));
            QuestGen.quest.AddPart(questPart);
        }
    }
}