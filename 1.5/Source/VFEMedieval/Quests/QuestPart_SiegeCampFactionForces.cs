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
        public List<PawnKindDef> defenderPawns;
        public List<Pawn> defenderPawnsGenerated;
        public Faction siteFaction;
        public float points;
        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (site.Map != null)
            {
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
            Scribe_Collections.Look(ref defenderPawns, "defenderPawns",  LookMode.Def);
            Scribe_Collections.Look(ref defenderPawnsGenerated, "defenderPawnsGenerated", LookMode.Reference);
            Scribe_References.Look(ref siteFaction, "siteFaction");
            Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent");
            Scribe_Values.Look(ref allAlliesDownedOrDeadSignalSent, "allAlliesDownedOrDeadSignalSent");
            Scribe_Values.Look(ref points, "points");
        }

        private bool CheckAllDefendersDefeated()
        {
            if (defenderPawnsGenerated.NullOrEmpty())
            {
                return true;
            }

            foreach (var defender in defenderPawnsGenerated)
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