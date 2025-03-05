using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VFEMedieval
{
    public class QuestPart_FactionForces : QuestPartActivable
    {
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
                // do enemies fleeing/killing check
                Log.Message("Ticking??: " + enemyUnitPawns.ToStringSafeEnumerable() + " - " + friendlyUnitPawns.ToStringSafeEnumerable());
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
        }
    }
}