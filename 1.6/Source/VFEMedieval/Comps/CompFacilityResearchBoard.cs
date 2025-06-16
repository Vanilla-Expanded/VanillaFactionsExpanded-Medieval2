
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Verse.AI.ReservationManager;

namespace VFEMedieval
{
    public class CompFacilityResearchBoard : CompFacility
    {
        public override List<StatModifier> StatOffsets => AllLinkablesInUse ? StatOffsetsInUse : base.StatOffsets;

        private List<StatModifier> StatOffsetsInUse
        {
            get
            {
                var statOffsets = base.StatOffsets.Select(x => x.Clone() as StatModifier).ToList();
                foreach (var statOffset in statOffsets)
                {
                    statOffset.value += 0.05f;
                }
                return statOffsets;
            }
        }

        private bool AllLinkablesInUse => this.LinkedBuildings.TrueForAll(x => InUse(x));

        private bool InUse(Thing thing)
        {
            var reservations = this.parent.Map != null
            ? this.parent.Map.reservationManager.ReservationsReadOnly.Where(x => x.Target.Thing == thing)
            : Enumerable.Empty<Reservation>();
            var claimants = reservations.Select(x => x.Claimant).ToList();
            foreach (var claimant in claimants)
            {
                var pawnPosition = claimant.Position;
                if (pawnPosition == thing.Position || pawnPosition == thing.InteractionCell)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
