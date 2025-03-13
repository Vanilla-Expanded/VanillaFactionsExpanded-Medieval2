using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class VFEMedieval : Mod
    {
        public VFEMedieval(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFEMedieval");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            GetSettings<VFEMedievalSettings>();
        }

        public static Harmony harmonyInstance;
    }

    public class VFEMedievalSettings : ModSettings
    {
        public static int merchantCaravanSpawnCount = 6;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            merchantCaravanSpawnCount = (int)listingStandard.SliderLabeled("VFEM2_MerchantCaravanSpawnCount".Translate() 
                + ": " + merchantCaravanSpawnCount.ToString(), merchantCaravanSpawnCount, 1, 20);
            listingStandard.End();
            VFEM_DefOf.VFEM2_MerchantGuildCaravan.initialSpawnCount = new IntRange(merchantCaravanSpawnCount, merchantCaravanSpawnCount);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref merchantCaravanSpawnCount, "merchantCaravanSpawnCount", 6);
            if (VFEM_DefOf.VFEM2_MerchantGuildCaravan != null)
            {
                VFEM_DefOf.VFEM2_MerchantGuildCaravan.initialSpawnCount = new IntRange(merchantCaravanSpawnCount, merchantCaravanSpawnCount);
            }
        }
    }
}