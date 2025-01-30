using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    public class CaravanArrivalAction_Barter : CaravanArrivalAction
    {
        private MerchantGuild merchantGuild;

        public override string Label => "TradeWithSettlement".Translate(merchantGuild.Label);

        public override string ReportString => "CaravanTrading".Translate(merchantGuild.Label);

        public CaravanArrivalAction_Barter()
        {
        }

        public CaravanArrivalAction_Barter(MerchantGuild settlement)
        {
            this.merchantGuild = settlement;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            if (!floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (merchantGuild != null && merchantGuild.Tile != destinationTile)
            {
                return false;
            }
            return CanTradeWith(caravan, merchantGuild);
        }

        public override void Arrived(Caravan caravan)
        {
            CameraJumper.TryJumpAndSelect(caravan);
            Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, this.merchantGuild.Faction, 
                this.merchantGuild.TraderKind);
            Caravan_PathFollower_ExposeData_Patch.merchantsToFollow.Remove(caravan.pather);
            Find.WindowStack.Add(new Dialog_Barter(playerNegotiator, this.merchantGuild));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref merchantGuild, "settlement");
        }

        public static FloatMenuAcceptanceReport CanTradeWith(Caravan caravan, MerchantGuild merchantGuild)
        {
            return merchantGuild != null && merchantGuild.Spawned
                && merchantGuild.Faction != null && merchantGuild.Faction != Faction.OfPlayer 
                && !merchantGuild.Faction.def.permanentEnemy && !merchantGuild.Faction.HostileTo(Faction.OfPlayer) 
                && merchantGuild.CanTradeNow && HasNegotiator(caravan, merchantGuild);
        }

        public static bool HasNegotiator(Caravan caravan, MerchantGuild merchantGuild)
        {
            Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan, merchantGuild.Faction, 
                merchantGuild.TraderKind);
            if (pawn != null)
            {
                return !pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
            }
            return false;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MerchantGuild merchantGuild)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanTradeWith(caravan, merchantGuild), 
                () => new CaravanArrivalAction_Barter(merchantGuild), 
                "VFEM2_BarterWith".Translate(merchantGuild.Label), caravan, merchantGuild.Tile, merchantGuild,
                delegate
                {
                    Caravan_PathFollower_ExposeData_Patch.merchantsToFollow[caravan.pather] = merchantGuild;
                });
        }
    }
}