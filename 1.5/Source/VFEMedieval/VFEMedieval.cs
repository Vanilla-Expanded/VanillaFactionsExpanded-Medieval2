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
        public static float merchantCaravanSpawnCount = 3f;

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            merchantCaravanSpawnCount = listingStandard.SliderLabeled("VFEM2_MerchantCaravanSpawnCount".Translate() 
                + ": " + merchantCaravanSpawnCount.ToString("F2"), merchantCaravanSpawnCount, 1f, 10f);
            listingStandard.End();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref merchantCaravanSpawnCount, "merchantCaravanSpawnCount", 3f);
        }
    }
}