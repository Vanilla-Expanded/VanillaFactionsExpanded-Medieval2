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
    public abstract class QuestNode_MakeFactionForcesBase : QuestNode
    {
        public override bool TestRunInt(Slate slate)
        {
            return true;
        }

        public static List<Pawn> GeneratePawnList(Faction faction, float points, Site site)
        {
            PawnGroupMakerParms pawnGroupMakerParms = GetParms(faction, points, site);
            var minPoints = faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms);
            points = minPoints < float.MaxValue ? Mathf.Max(points, minPoints) : points;
            pawnGroupMakerParms.points = points;
            List<Pawn> generatedPawns = new List<Pawn>();
            while (generatedPawns.Any() is false && points < 99999)
            {
                points += 50f;
                pawnGroupMakerParms.points = points;
                generatedPawns = GeneratePawns(pawnGroupMakerParms, false).ToList();
            }
            return generatedPawns;
        }
        
        public static IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, bool warnOnZeroResults = true)
        {
            if (parms.groupKind == null)
            {
                Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
                yield break;
            }
            if (parms.faction == null)
            {
                Log.Error("Tried to generate pawn kinds with null faction. parms=" + parms);
                yield break;
            }
            if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
            {
                Log.Error(string.Concat("Faction ", parms.faction, " of def ", parms.faction.def, " has no PawnGroupMakers."));
                yield break;
            }
            if (!PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker))
            {
                yield break;
            }
            foreach (Pawn item in pawnGroupMaker.GeneratePawns(parms, warnOnZeroResults))
            {
                yield return item;
            }
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

        protected string FormatPawnListToString(List<Pawn> pawns)
        {
            if (pawns == null || !pawns.Any())
            {
                return "";
            }
            return pawns.GroupBy(p => p.kindDef).Select(group => $"{group.Count()} {group.Key.label}").ToCommaList();
        }
    }
}