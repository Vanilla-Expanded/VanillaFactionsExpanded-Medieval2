using RimWorld;
using Verse;

namespace VFEMedieval
{
    [DefOf]
    public static class VFEM_DefOf
    {

        static VFEM_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VFEM_DefOf));
        }

        public static HediffDef VFEM2_Sting;

        public static ThoughtDef VFEM2_StingMoodDebuff;

        public static ThingDef VFEM2_Honey;
        public static ThingDef VFEM2_Apiary;

        public static DamageDef VFEM2_DamageSting;

        public static JobDef VFEM2_TendToApiary;
        public static JobDef VFEM2_TakeHoneyOutOfApiary;


    }
}