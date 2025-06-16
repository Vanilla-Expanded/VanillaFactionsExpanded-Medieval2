using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;



namespace VFEMedieval
{


    [HarmonyPatch(typeof(DrugPolicyDatabase))]
    [HarmonyPatch("GenerateStartingDrugPolicies")]
    public static class VFEMedieval_DrugPolicyDatabase_GenerateStartingDrugPolicies_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref List<DrugPolicy> ___policies)
        {
            // Count wine as a recreational drug
            for (int i = 0; i < ___policies.Count; i++)
            {
                var policy = ___policies[i];
                if (policy.label == "SocialDrugs".Translate() || policy.label == "OneDrinkPerDay".Translate())
                {
                    policy[VFEM_DefOf.VFEM2_Wine].allowedForJoy = true;
                }
            }
        }
    }








}
