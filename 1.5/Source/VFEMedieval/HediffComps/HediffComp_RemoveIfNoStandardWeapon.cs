using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_RemoveIfNoWeapon : HediffComp
    {
        public HediffCompProperties_RemoveIfNoWeapon Props => (HediffCompProperties_RemoveIfNoWeapon)props;

        public override bool CompShouldRemove
        {
            get
            {
                return Pawn.equipment?.Primary == null || Pawn.equipment.Primary.def != Props.weaponDef;
            }
        }
    }
}