using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
namespace VFEMedieval
{
    public class QuestPart_SpawnRaidOnFail : QuestPartActivable
    {
        public Faction faction;
        public List<PawnKindDef> raidersList;
        public Map map;
        public string inSignal;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref raidersList, "raidersList", LookMode.Def);
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref inSignal, "inSignal");
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (!(signal.tag == inSignal))
            {
                return;
            }
            SpawnRaid();
            raidersList.Clear();
        }

        private void SpawnRaid()
        {
            var pawns = raidersList.Select(x => PawnGenerator.GeneratePawn(x, faction)).ToList();
            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = faction;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
            parms.raidArrivalMode.Worker.Arrive(pawns, parms);
            var worker = IncidentDefOf.RaidEnemy.Worker as IncidentWorker_RaidEnemy;
            TaggedString letterLabel = worker.GetLetterLabel(parms);
            TaggedString letterText = worker.GetLetterText(parms, pawns);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, worker.GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
            List<TargetInfo> list = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list2 = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
                List<Pawn> list3 = list2.MaxBy((List<Pawn> x) => x.Count);
                if (list3.Any())
                {
                    list.Add(list3[0]);
                }
                for (int i = 0; i < list2.Count; i++)
                {
                    if (list2[i] != list3 && list2[i].Any())
                    {
                        list.Add(list2[i][0]);
                    }
                }
            }
            else if (pawns.Any())
            {
                foreach (Pawn item in pawns)
                {
                    list.Add(item);
                }
            }
            worker.SendStandardLetter(letterLabel, letterText, worker.GetLetterDef(), parms, list);
            if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer)
            {
                parms.raidStrategy.Worker.MakeLords(parms, pawns);
            }
        }
    }
}