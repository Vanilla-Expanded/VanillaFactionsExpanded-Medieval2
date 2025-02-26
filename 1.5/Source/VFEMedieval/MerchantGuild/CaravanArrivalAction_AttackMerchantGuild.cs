using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    public class CaravanArrivalAction_AttackMerchantGuild : CaravanArrivalAction
    {
        private MerchantGuild merchantGuild;

        public override string Label => "AttackSettlement".Translate(merchantGuild.Label);

        public override string ReportString => "CaravanAttacking".Translate(merchantGuild.Label);

        public CaravanArrivalAction_AttackMerchantGuild()
        {
        }

        public CaravanArrivalAction_AttackMerchantGuild(MerchantGuild merchantGuild)
        {
            this.merchantGuild = merchantGuild;
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
            return CanAttack(caravan, merchantGuild);
        }

        public override void Arrived(Caravan caravan)
        {
            merchantGuild.Attack(caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref merchantGuild, "merchantGuild");
        }

        public static FloatMenuAcceptanceReport CanAttack(Caravan caravan, MerchantGuild merchantGuild)
        {
            if (merchantGuild == null || !merchantGuild.Spawned || !merchantGuild.Attackable)
            {
                return false;
            }
            return true; 
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MerchantGuild merchantGuild)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(caravan, merchantGuild),
                () => new CaravanArrivalAction_AttackMerchantGuild(merchantGuild), "AttackSettlement".Translate(merchantGuild.Label), caravan, merchantGuild.Tile, merchantGuild);
        }
    }
}