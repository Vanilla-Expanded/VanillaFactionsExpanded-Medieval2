using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VFEMedieval
{

    public class ApparelExtension : DefModExtension
    {
        public bool showOpenWhenNotShowingWeapons;
    }

    [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class Pawn_DraftController_Drafted_Patch
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            __instance.pawn.TryUpdateGraphics();
        }

        public static void TryUpdateGraphics(this Pawn pawn)
        {
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                });
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class Pawn_JobTracker_StartJob_Patch
    {
        public static void Postfix(Pawn_JobTracker __instance)
        {
            __instance.pawn.TryUpdateGraphics();
        }
    }
    
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
    public static class Pawn_JobTracker_EndCurrentJob_Patch
    {
        public static void Postfix(Pawn_JobTracker __instance)
        {
            __instance.pawn.TryUpdateGraphics();

        }
    }
    
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.MakeDowned))]
    public static class Pawn_HealthTracker_MakeDowned_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance)
        {
            __instance.pawn.TryUpdateGraphics();
        }
    }
    
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.MakeUndowned))]
    public static class Pawn_HealthTracker_MakeUndowned_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance)
        {
            __instance.pawn.TryUpdateGraphics();
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Ldloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(typeof(ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch), "TryOverridePath"));
                }
            }
        }

        public static string TryOverridePath(string path, Apparel apparel)
        {
            if (apparel.Wearer is Pawn pawn)
            {
                var extension = apparel.def.GetModExtension<ApparelExtension>();
                if (extension != null && extension.showOpenWhenNotShowingWeapons 
                    && PawnRenderUtility.CarryWeaponOpenly(pawn) is false)
                {
                    return path + "_Open";
                }
            }
            return path;
        }
    }
}
