using Verse;

namespace VFEMedieval
{
    public class HediffCompProperties_RemoveIfNoWeaponOrDowned : HediffCompProperties
    {
        public ThingDef weaponDef;

        public HediffCompProperties_RemoveIfNoWeaponOrDowned()
        {
            compClass = typeof(HediffComp_RemoveIfNoWeaponOrDowned);
        }
    }
}