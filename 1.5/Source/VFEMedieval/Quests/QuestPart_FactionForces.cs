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
        public List<Pawn> enemyUnitPawns;
        public List<Pawn> friendlyUnitPawns;
        private bool spawned;
        public Faction friendlyFaction;
        public Faction enemyFaction;
        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (site.Map != null)
            {
                spawned = true;
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
            Scribe_Values.Look(ref spawned, "spawned");
            var lookMode = spawned ? LookMode.Reference : LookMode.Deep;
            Scribe_Collections.Look(ref enemyUnitPawns, "enemyUnitPawns", lookMode);
            Scribe_Collections.Look(ref friendlyUnitPawns, "friendlyUnitPawns", lookMode);
            Scribe_References.Look(ref friendlyFaction, "friendlyFaction");
            Scribe_References.Look(ref enemyFaction, "enemyFaction");
            Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent");
        }

        private bool CheckAllEnemiesDefeated()
        {
            if (enemyUnitPawns.NullOrEmpty())
            {
                return true;
            }

            foreach (var enemy in enemyUnitPawns)
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