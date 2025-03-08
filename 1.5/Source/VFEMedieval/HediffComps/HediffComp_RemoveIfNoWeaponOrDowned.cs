using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class HediffComp_RemoveIfNoWeaponOrDowned : HediffComp
    {
        public HediffCompProperties_RemoveIfNoWeaponOrDowned Props => (HediffCompProperties_RemoveIfNoWeaponOrDowned)props;

        public override bool CompShouldRemove => Pawn.Downed || Pawn.equipment?.Primary == null
        || Pawn.equipment.Primary.def != Props.weaponDef;
    }
}