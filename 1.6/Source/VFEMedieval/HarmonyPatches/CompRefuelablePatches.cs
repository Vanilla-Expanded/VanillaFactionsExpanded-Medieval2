using RimWorld;
using System.Reflection;
using Verse;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;



namespace VFEMedieval
{
    [HarmonyPatch]
    public static class CompRefuelablePatches
    {
        public static float ModifyFuelConsumptionRate(float originalRate, CompRefuelable instance)
        {
            if (instance.parent.IsLinkedTo(VFEM_DefOf.VFEM2_ForgeBellows))
            {
                return originalRate * 0.8f;
            }
            return originalRate;
        }

        public static bool IsLinkedTo(this ThingWithComps thingWithComps, ThingDef thingDef)
        {
            var comp = thingWithComps.TryGetComp<CompAffectedByFacilities>();
            if (comp != null)
            {
                if (comp.LinkedFacilitiesListForReading.Any(x => x.def == thingDef))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(CompRefuelable).GetMethod(nameof(CompRefuelable.CompInspectStringExtra));
            yield return typeof(CompRefuelable).GetMethod("get_ConsumptionRatePerTick",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(typeof(CompProperties_Refuelable).GetField(nameof(CompProperties_Refuelable.fuelConsumptionRate))))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // Load CompRefuelable instance
                    yield return CodeInstruction.Call(typeof(CompRefuelablePatches), nameof(ModifyFuelConsumptionRate));
                }
            }
        }
    }








}
