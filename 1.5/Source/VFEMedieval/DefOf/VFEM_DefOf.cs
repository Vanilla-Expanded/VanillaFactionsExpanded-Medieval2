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
        public static HediffDef HeartAttack;

        public static ThoughtDef VFEM2_StingMoodDebuff;

        public static ThingDef VFEM2_Honey;
        public static ThingDef VFEM2_Apiary;
        public static ThingDef VFEM2_Wine;
        public static ThingDef Bow_Great;
   
        public static ThingDef MeleeWeapon_LongSword;
        public static ThingDef VFEM2_TimberedWall;        
        public static ThingDef VFEM2_CastleWall;
        public static ThingDef VFEM2_DamagedCastleWall;
        public static ThingDef VFEM2_LowCastleWall;
        public static ThingDef VFEM2_DamagedLowCastleWall;
        public static ThingDef VFEM2_SmithingAnvil;
        public static ThingDef VFEM2_StoneClamp;
        public static ThingDef VFEM2_CarvingBoard;
        public static ThingDef VFEM2_StonePolisher;
        public static ThingDef VFEM2_ArcheryTarget;

        public static DamageDef VFEM2_DamageSting;

        public static JobDef VFEM2_TendToApiary;
        public static JobDef VFEM2_TakeHoneyOutOfApiary;

        public static FleckDef VFEM2_ArrowThrowable;

        public static BodyPartDef Kidney;

        public static ThingDef VFEM2_ForgeBellows;

        public static ThingDef VFEM2_MannequinStand;

        public static FactionDef VFEM2_MerchantGuild;

        public static WorldObjectDef VFEM2_MerchantGuildCaravan;

        public static TraderKindDef VFEM2_MerchantGuildTrader;

        public static ThingDef Grimworld_FlintlockSmoke;

        public static ThingCategoryDef ResourcesRaw;
    }
}