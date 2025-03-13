using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VFEMedieval
{
    public class QuestPart_FactionForces : QuestPartActivable
    {
        private bool allEnemiesDefeatedSignalSent;
        public Site site;
        public List<PawnKindDef> enemyUnitPawns;
        public List<PawnKindDef> friendlyUnitPawns;
        public List<Pawn> enemyUnitPawnsGenerated;
        public Faction friendlyFaction;
        public Faction enemyFaction;
        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (site.Map != null)
            {
                if (!allEnemiesDefeatedSignalSent)
                {
                    if (CheckAllEnemiesDefeated())
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
            Scribe_Collections.Look(ref enemyUnitPawns, "enemyUnitPawns", LookMode.Def);
            Scribe_Collections.Look(ref enemyUnitPawnsGenerated, "enemyUnitPawnsGenerated", LookMode.Reference);
            Scribe_Collections.Look(ref friendlyUnitPawns, "friendlyUnitPawns", LookMode.Def);
            Scribe_References.Look(ref friendlyFaction, "friendlyFaction");
            Scribe_References.Look(ref enemyFaction, "enemyFaction");
            Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent");
        }

        private bool CheckAllEnemiesDefeated()
        {
            if (enemyUnitPawnsGenerated.NullOrEmpty())
            {
                return true;
            }

            foreach (var enemy in enemyUnitPawnsGenerated)
            {
                if (!enemy.Dead && !enemy.Downed && enemy.MentalStateDef != MentalStateDefOf.PanicFlee)
                {
                    return false;
                }
            }
            return true;
        }
    }
}