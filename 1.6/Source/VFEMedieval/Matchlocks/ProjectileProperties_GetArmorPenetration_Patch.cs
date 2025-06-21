using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(ProjectileProperties), "GetArmorPenetration", typeof(Thing), typeof(StringBuilder))]
    public static class ProjectileProperties_GetArmorPenetration_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var codes = instructions.ToList();
            bool found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (i > 2 && codes[i - 2].opcode == OpCodes.Ldloc_0
                    && codes[i - 1].opcode == OpCodes.Ldc_R4 && codes[i - 1].operand is float floatOperand 
                    && floatOperand == 0.0f && codes[i].opcode == OpCodes.Bge_Un_S && codes[i].operand is Label label)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(ProjectilePropertiesMatchLock));
                    yield return new CodeInstruction(OpCodes.Brtrue, label);
                    found = true;
                }
            }

            if (found is false)
            {
                Log.Error("VFEMedieval: Transpiler for ProjectileProperties.GetArmorPenetration_Patch failed to find pattern. Patch might not work correctly.");
            }
        }
    }
}