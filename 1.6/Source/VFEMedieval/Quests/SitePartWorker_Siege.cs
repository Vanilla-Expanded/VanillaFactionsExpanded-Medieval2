using RimWorld;

namespace VFEMedieval
{
    public class SitePartWorker_Siege : SitePartWorker
    {
        public override bool FactionCanOwn(Faction faction)
        {
            return base.FactionCanOwn(faction) && faction.HostileTo(Faction.OfPlayer);
        }
    }
}