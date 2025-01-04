using HarmonyLib;
using System.Reflection;
using Verse;

namespace VFEMedieval
{
    public class VFEMedieval : Mod
    {
        public VFEMedieval(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFEMedieval");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static Harmony harmonyInstance;
    }
}