using Verse;

namespace VFEMedieval
{
    public class HediffCompProperties_RemoveIfNoWeapon : HediffCompProperties
    {
        public ThingDef weaponDef;

        public HediffCompProperties_RemoveIfNoWeapon()
        {
            compClass = typeof(HediffComp_RemoveIfNoWeapon);
        }
    }
}