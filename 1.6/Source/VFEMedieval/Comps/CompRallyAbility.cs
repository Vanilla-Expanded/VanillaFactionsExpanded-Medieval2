using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class CompRallyAbility : ThingComp
    {
        public CompProperties_RallyAbility Props => props as CompProperties_RallyAbility;

        public Gizmo GetRallyGizmo()
        {
            var enemiesPresent = EnemiesPresent();
            var alreadyHasHediff = AlreadyHasHediff();
            return new Command_Action()
            {
                defaultLabel = Props.gizmoLabel,
                defaultDesc = Props.gizmoDescription,
                icon = ContentFinder<Texture2D>.Get(Props.icon),
                action = ActivateAbility,
                disabled = enemiesPresent is false || alreadyHasHediff,
                disabledReason = enemiesPresent is false ? Props.disabledReason : Props.disabledReasonAlreadyUsing
            };
        }

        private bool AlreadyHasHediff()
        {
            Pawn wearer = Wearer;
            return wearer != null && wearer.health.hediffSet.HasHediff(Props.hediffWearer);
        }

        private bool EnemiesPresent()
        {
            Pawn wearer = Wearer;
            if (wearer != null)
            {
                return wearer.Map.attackTargetsCache.GetPotentialTargetsFor(wearer).Any(t => t.Thing is Pawn targetPawn && targetPawn.HostileTo(wearer.Faction));
            }
            return false;
        }

        private void ActivateAbility()
        {
            if (!EnemiesPresent()) return;

            ApplyRallyHediffToWearer();
        }

        private void ApplyRallyHediffToWearer()
        {
            Pawn wearer = Wearer;
            if (wearer != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.hediffWearer, wearer);
                HediffComp_Disappears disappearsComp = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappearsComp != null)
                {
                    disappearsComp.ticksToDisappear = 30000;
                }
                wearer.health.AddHediff(hediff);
            }
        }


        public Pawn Wearer => parent.ParentHolder is Pawn_EquipmentTracker equipmentTracker ? equipmentTracker.pawn : null;
    }
    public class CompProperties_RallyAbility : CompProperties
    {
        public string gizmoLabel;
        public string gizmoDescription;
        public string icon;
        public HediffDef hediffWearer;
        public string disabledReason;
        public string disabledReasonAlreadyUsing;
        public CompProperties_RallyAbility()
        {
            compClass = typeof(CompRallyAbility);
        }
    }
}
