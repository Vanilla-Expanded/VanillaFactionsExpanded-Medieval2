﻿using RimWorld;
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
        public static ThingDef Bow_Great;
        public static ThingDef MeleeWeapon_LongSword;

        public static DamageDef VFEM2_DamageSting;

        public static JobDef VFEM2_TendToApiary;
        public static JobDef VFEM2_TakeHoneyOutOfApiary;

        public static FleckDef VFEM2_ArrowThrowable;

        public static PawnKindDef VFEM_SellSword;
    }
}