using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
	public class JobDriver_IgniteWithTorches : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.None, closeIfDowned: false, 0.95f);
			yield return Toils_Combat.CastVerb(TargetIndex.A);
		}
	}
}
