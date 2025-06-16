using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using VEF.Planet;

namespace VFEMedieval
{
    [HotSwappable]
    public class CaravanArrivalAction_Barter : CaravanArrivalAction_MovingBase
    {
        public override string Label => "TradeWithSettlement".Translate(movingBase.Label);

        public override string ReportString => "CaravanTrading".Translate(movingBase.Label);

        public CaravanArrivalAction_Barter()
        {
        }

        public CaravanArrivalAction_Barter(MerchantGuild merchantGuild)
        {
            this.movingBase = merchantGuild;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            return base.StillValid(caravan, destinationTile) && CanTradeWith(caravan, movingBase as MerchantGuild);
        }

        public override void Arrived(Caravan caravan)
        {
            base.Arrived(caravan);
            CameraJumper.TryJumpAndSelect(caravan);
            var merchantGuild = movingBase as MerchantGuild;
            Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, merchantGuild.Faction,
                merchantGuild.TraderKind);
            Find.WindowStack.Add(new Dialog_Barter(playerNegotiator, merchantGuild));
        }

        public static FloatMenuAcceptanceReport CanTradeWith(Caravan caravan, MerchantGuild merchantGuild)
        {
            return merchantGuild != null && merchantGuild.Spawned
                && merchantGuild.Faction != null && !merchantGuild.Faction.HostileTo(Faction.OfPlayer)
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
                () => CreateCaravanArrivalAction(new CaravanArrivalAction_Barter(merchantGuild), caravan, merchantGuild),
                "VFEM2_BarterWith".Translate(merchantGuild.Label), caravan, merchantGuild.Tile, merchantGuild,
                delegate (Action action)
                {
                    if (caravan.Tile == merchantGuild.Tile)
                    {
                        Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, merchantGuild.Faction,
                            merchantGuild.TraderKind);
                        Find.WindowStack.Add(new Dialog_Barter(playerNegotiator, merchantGuild));
                    }
                    else
                    {
                        action();
                    }
                });
        }
    }
}