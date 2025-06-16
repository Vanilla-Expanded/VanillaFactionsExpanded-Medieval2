using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
	public class JobGiver_BurnAndStealColony : ThinkNode_JobGiver
	{
		public static bool HasTorches(Pawn pawn)
		{
			var torches = pawn.apparel.WornApparel.Where(a => a.def == VFEM_DefOf.VFEM2_Apparel_TorchBelt).FirstOrDefault();
			if (torches != null && torches.TryGetComp<CompApparelReloadable>().RemainingCharges > 0)
			{
				return true;
			}
			return false;
		}

		public static Verb GetVerbFromTorches(Pawn pawn)
		{
			var torches = pawn.apparel.WornApparel.Where(a => a.def == VFEM_DefOf.VFEM2_Apparel_TorchBelt).FirstOrDefault();
			if (torches != null)
			{
				foreach (var verb in torches.TryGetComp<CompApparelReloadable>().AllVerbs)
				{
					return verb;
				}
			}
			return null;
		}

		public static bool TryFindBestItemToSteal(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null, Danger danger = Danger.Some)
		{
			if (map == null)
			{
				//Log.Message(" - TryFindBestItemToSteal - item = null; - 2", true);
				item = null;
				//Log.Message(" - TryFindBestItemToSteal - return false; - 3", true);
				return false;
			}
			if (thief != null && !thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				//Log.Message(" - TryFindBestItemToSteal - item = null; - 5", true);
				item = null;
				//Log.Message(" - TryFindBestItemToSteal - return false; - 6", true);
				return false;
			}
			if ((thief != null && !map.reachability.CanReachMapEdge(thief.Position, TraverseParms.For(thief, Danger.Some)))
				|| (thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some))))
			{
				//Log.Message(" - TryFindBestItemToSteal - item = null; - 8", true);
				item = null;
				//Log.Message(" - TryFindBestItemToSteal - return false; - 9", true);
				return false;
			}
			Predicate<Thing> validator = delegate (Thing t)
			{
				if (!t.def.defName.ToLower().Contains("chunk"))
				{
					//Log.Message("Item candidate: " + t, true);
				}
				if (t.def.IsCorpse)
				{
					return false;
				}
				if (thief != null && !thief.CanReserve(t))
				{
					//Log.Message(" - TryFindBestItemToSteal - return false; - 11", true);
					return false;
				}
				if (disallowed != null && disallowed.Contains(t))
				{
					//Log.Message(" - TryFindBestItemToSteal - return false; - 13", true);
					return false;
				}
				if (!t.def.stealable)
				{
					//Log.Message(" - TryFindBestItemToSteal - return false; - 15", true);
					return false;
				}
				if (t.Faction == thief.Faction)
				{
					return false;
				}
				if (t.def.IsWeapon)
				{
					return false;
				}
				if (GetValue(t) < 10f)
				{
					return false;
				}
				return (!t.IsBurning()) ? true : false;
				//Log.Message(" - TryFindBestItemToSteal - }; - 17", true);
			};

			item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable),
					PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, danger), maxDist, validator, (Thing x) => GetValue(x), 15, 15);
			if (item == null)
			{
				//Log.Message("Item is null", true);
			}
			else
			{
				//Log.Message(item + " - " + GetValue(item), true);
			}
			return item != null;
		}


		public static float GetValue(Thing thing)
		{
			return thing.MarketValue * (float)thing.stackCount;
		}
		public override Job TryGiveJob(Pawn pawn)
		{
			//Log.Message(pawn + " - " + pawn.mindState.duty);
			if (!pawn.HostileTo(Faction.OfPlayer))
			{
				//Log.Message("0 - " + pawn + " - null", true);
				return null;
			}

			if (RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
			{
				//Log.Message(pawn + " found spot: " + spot, true);
				if (TryFindBestItemToSteal(pawn.Position, pawn.Map, 50f, out Thing item, pawn))// && !GenAI.InDangerousCombat(pawn))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Steal);
					job.targetA = item;
					job.targetB = spot;
					job.canBashDoors = true;
					job.canBashFences = true;
					job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
					//Log.Message("3 - " + pawn + " - " + job, true);
					return job;
				}
				//Log.Message(pawn + " cant find item to steal", true);
			}
			bool flag = pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && pawn.HostileTo(Faction.OfPlayer);
			CellRect cellRect = CellRect.CenteredOn(pawn.Position, 5);
			for (int i = 0; i < 35; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				if (!randomCell.InBounds(pawn.Map))
				{
					continue;
				}
				Building edifice = randomCell.GetEdifice(pawn.Map);
				if (edifice != null && TrashUtility.ShouldTrashBuilding(pawn, edifice) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
				{
					Job job = TrashJob(pawn, edifice);
					if (job != null)
					{
						//Log.Message("1 - " + pawn + " - " + job, true);
						return job;
					}
				}
				if (flag)
				{
					Plant plant = randomCell.GetPlant(pawn.Map);
					if (plant != null && TrashUtility.ShouldTrashPlant(pawn, plant) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
					{
						Job job2 = TrashJob(pawn, plant);
						if (job2 != null)
						{
							//Log.Message("2 - " + pawn + " - " + job2, true);

							return job2;
						}
					}
				}
			}

			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count == 0)
			{
				//Log.Message("4 - " + pawn + " - null", true);
				return null;
			}
			foreach (var building in allBuildingsColonist.OrderBy(x => IntVec3Utility.DistanceTo(x.Position, pawn.Position)).Take(10).InRandomOrder())
			{
				if (TrashUtility.ShouldTrashBuilding(pawn, building, true) && pawn.CanReach(building, PathEndMode.Touch, Danger.None))
				{
					Job job = TrashJob(pawn, building, true);
					if (job != null)
					{
						//Log.Message("5 - " + pawn + " - " + job, true);
						return job;
					}
				}
			}
			if (RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot2))
			{
				//Log.Message(pawn + " found spot: " + spot2, true);
				if (TryFindBestItemToSteal(pawn.Position, pawn.Map, 100f, out Thing item, pawn, danger: Danger.None))// && !GenAI.InDangerousCombat(pawn))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Steal);
					job.targetA = item;
					job.targetB = spot2;
					job.canBashDoors = true;
					job.canBashFences = true;
					job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
					//Log.Message("6 - " + pawn + " - " + job, true);
					return job;
				}
				//Log.Message(pawn + " cant find item to steal", true);
			}
			//Log.Message("7 - " + pawn + " - null", true);
			return null;
		}

		public static Job TrashJob(Pawn pawn, Thing t, bool allowPunchingInert = false, bool killIncappedTarget = false)
		{
			if (t is Plant)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Ignite, t);
				FinalizeTrashJob(job);
				return job;
			}
			Job job3 = null;
			if (pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && t.FlammableNow && !t.IsBurning() && !(t is Building_Door))
			{
				if (HasTorches(pawn))
				{
					job3 = JobMaker.MakeJob(VFEM_DefOf.VFEM_IgniteWithTorches, t);
					job3.verbToUse = GetVerbFromTorches(pawn);
				}
				else
				{
					job3 = JobMaker.MakeJob(JobDefOf.Ignite, t);
				}
			}
			else
			{
				job3 = JobMaker.MakeJob(JobDefOf.AttackMelee, t);
			}
			job3.killIncappedTarget = killIncappedTarget;
			FinalizeTrashJob(job3);
			return job3;
		}
		private static void FinalizeTrashJob(Job job)
		{
			job.expiryInterval = TrashJobCheckOverrideInterval.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
		}

		private static readonly IntRange TrashJobCheckOverrideInterval = new IntRange(450, 500);

	}
}
