using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HotSwappable]
    public class MerchantCaravan : Caravan
    {
        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(base.def.expandingIconTexture, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public override Texture2D ExpandingIcon => ContentFinder<Texture2D>.Get(base.def.texture);

        public override void PostAdd()
        {
            try
            {
                base.PostAdd();
            }
            catch { }
            TryStartPathing();
        }

        public override string GetInspectString()
        {
            return "";
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                if ((g is Gizmo_CaravanInfo) is false)
                {
                    yield return g;
                }
            }
        }

        public void GeneratePawns()
        {
            var pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Trader,
                faction = Faction,
                points = TraderCaravanUtility.GenerateGuardPoints(),
                dontUseSingleUseRocketLaunchers = true
            }).ToList();

            foreach (var pawn in pawns)
            {
                if (!this.ContainsPawn(pawn))
                {
                    this.AddPawn(pawn, true);
                }
                Find.WorldPawns.AddPawn(pawn);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0 && pawns.InnerListForReading.Count == 0)
            {
                GeneratePawns();
            }
            if (pather.MovingNow is false)
            {
                TryStartPathing();
            } 
        }

        private void TryStartPathing()
        {
            if (TileFinder.TryFindPassableTileWithTraversalDistance(Tile, 5, 30, out var result))
            {
                int num = CaravanUtility.BestGotoDestNear(result, this);
                if (num >= 0)
                {
                    this.pather.StartPath(num, null, repathImmediately: true);
                }
            }
        }
    }
}