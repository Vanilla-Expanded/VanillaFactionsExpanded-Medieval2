using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEMedieval
{
    public class IncidentWorker_PillageRaid : IncidentWorker_RaidEnemy
    {
        public override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Faction faction = null;
            if (Rand.Chance(0.5f))
            {
                faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_ClanSavage);
                if (!faction.HostileTo(Faction.OfPlayer))
                {
                    faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_ClanRough);
                    if (!faction.HostileTo(Faction.OfPlayer))
                    {
                        return false;
                    }
                }
            }
            else
            {
                faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_ClanRough);
                if (!faction.HostileTo(Faction.OfPlayer))
                {
                    faction = Find.FactionManager.FirstFactionOfDef(VFEM_DefOf.VFEM2_ClanSavage);
                    if (!faction.HostileTo(Faction.OfPlayer))
                    {
                        return false;
                    }
                }
            }
            parms.faction = faction;
            return true;
        }

        public override void ResolveRaidPoints(IncidentParms parms)
        {
            parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target) * 2f;
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            ResolveRaidPoints(parms);
            if (!TryResolveRaidFaction(parms))
            {
                return false;
            }
            if (!parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            ResolveRaidStrategy(parms, combat);
            ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            float points = parms.points;
            parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat,parms.target);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            GenerateRaidLoot(parms, points, list);
            TaggedString letterLabel = GetLetterLabel(parms);
            TaggedString letterText = GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any())
            {
                foreach (Pawn item in list)
                {
                    list2.Add(item);
                }
            }
            SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, list2);
            var ignitors = new List<Pawn>();
            foreach (var p in list)
            {
                if (p.apparel != null && Rand.Chance(0.5f))
                {
                    var throwableTorches = ThingMaker.MakeThing(VFEM_DefOf.VFEM2_Apparel_TorchBelt) as Apparel;
                    p.apparel.Wear(throwableTorches, false);
                    ignitors.Add(p);
                }
            }
            foreach (var pawn in list)
            {
                //Log.Message("Checking2 : " + pawn, true);
                if (pawn.RaceProps.Humanlike)
                {
                    if (Rand.Chance(0.3f) && !ignitors.Contains(pawn))
                    {
                        //Log.Message(pawn + " got duty: " + DutyDefOf.AssaultColony, true);
                        pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                    }
                    else
                    {
                        //Log.Message(pawn + " got duty: " + VFEM_DefOf.VFEM_BurnAndStealColony, true);
                        pawn.mindState.duty = new PawnDuty(VFEM_DefOf.VFEM_BurnAndStealColony);
                    }
                }
                else
                {
                    //Log.Message(pawn + " got duty: " + DutyDefOf.AssaultColony, true);
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                }
            }

            var lord = new LordJob_BurnAndStealColony(parms.faction, false, true, true, true, true);
            LordMaker.MakeNewLord(parms.faction, lord, (Map)parms.target, list);

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    Pawn pawn = list[j];
                    if (pawn.apparel != null && pawn.apparel.WornApparel.Any(ap => ap.def == ThingDefOf.Apparel_ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            return true;
        }



        public override string GetLetterLabel(IncidentParms parms)
        {
            return base.def.letterLabel;
        }

        public override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            return "VFEM_PillageRaidDesc".Translate(parms.faction);
        }

        public override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        public override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsRaidEnemy", Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}

