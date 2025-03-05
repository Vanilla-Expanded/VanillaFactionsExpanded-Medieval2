using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VFEMedieval
{
    public class QuestPart_SiegeCampFactionForces : QuestPartActivable
    {
        private bool allEnemiesDefeatedSignalSent;
        private bool allAlliesDownedOrDeadSignalSent; //New: Track allies downed/dead
        public Site site;
        public List<Pawn> defenderPawns; // defenders
        public List<Pawn> potentialAttackerPawns; // potential attackers (raid force)
        private bool spawned;
        public Faction siteFaction; // site faction (enemy)
        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (site.Map != null)
            {
                spawned = true;
                if (!allEnemiesDefeatedSignalSent)
                {
                    if (CheckAllDefendersDefeated()) // Check defenders instead of generic enemies
                    {
                        QuestUtility.SendQuestTargetSignals(site.questTags, "AllEnemiesDefeated", this.Named("SUBJECT")); //Keep signal name same for XML compatibility
                        allEnemiesDefeatedSignalSent = true;
                    }
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "site");
            Scribe_Values.Look(ref spawned, "spawned");
            var lookMode = spawned ? LookMode.Reference : LookMode.Deep;
            Scribe_Collections.Look(ref defenderPawns, "defenderPawns", lookMode); // defenders
            Scribe_Collections.Look(ref potentialAttackerPawns, "potentialAttackerPawns", LookMode.Deep); // potential attackers
            Scribe_References.Look(ref siteFaction, "siteFaction"); // siteFaction
            Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent");
            Scribe_Values.Look(ref allAlliesDownedOrDeadSignalSent, "allAlliesDownedOrDeadSignalSent"); // New: Save bool

        }

        private bool CheckAllDefendersDefeated() //Check only defenders
        {
            if (defenderPawns.NullOrEmpty())
            {
                return true;
            }

            foreach (var defender in defenderPawns)
            {
                if (!defender.Dead && !defender.Downed && defender.MentalStateDef != MentalStateDefOf.PanicFlee)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckAllPlayerPawnsLost() // New: Check if player lost all pawns
        {
            List<Pawn> playerPawns = site.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            if (playerPawns.NullOrEmpty())
            {
                return true; // No player pawns on map, consider them lost
            }
            foreach (var pawn in playerPawns)
            {
                if (!pawn.Dead && !pawn.Downed)
                {
                    return false; // Found at least one player pawn alive and not downed
                }
            }
            return true; // All player pawns are dead or downed
        }
    }
}