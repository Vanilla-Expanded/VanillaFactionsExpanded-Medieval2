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
        private bool allAlliesDownedOrDeadSignalSent;
        public Site site;
        public List<Pawn> defenderPawns;
        private bool spawned;
        public Faction siteFaction;
        public float points;
        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (site.Map != null)
            {
                spawned = true;
                if (!allEnemiesDefeatedSignalSent)
                {
                    if (CheckAllDefendersDefeated())
                    {
                        QuestUtility.SendQuestTargetSignals(site.questTags, "AllEnemiesDefeated", this.Named("SUBJECT"));
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
            Scribe_Collections.Look(ref defenderPawns, "defenderPawns", lookMode);
            Scribe_References.Look(ref siteFaction, "siteFaction");
            Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent");
            Scribe_Values.Look(ref allAlliesDownedOrDeadSignalSent, "allAlliesDownedOrDeadSignalSent");
            Scribe_Values.Look(ref points, "points");
        }

        private bool CheckAllDefendersDefeated()
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
    }
}