using Verse;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;

namespace VFEMedieval
{
    [StaticConstructorOnStartup]
    public static class RecipePatches
    {
        [HarmonyPatch(typeof(ITab_Bills), "FillTab")]
        public static class ITab_Bills_FillTab_Patch
        {
            [HarmonyPriority(int.MaxValue)]
            public static void Prefix(ITab_Bills __instance, out List<RecipeDef> __state)
            {
                var table = __instance.SelTable;
                __state = table.def.AllRecipes;
                if (table.IsLinkedTo(VFEM_DefOf.VFEM2_MannequinStand))
                {
                    table.def.allRecipesCached = table.def.AllRecipes.Select(x => x.ContractedRecipe()).ToList();
                }
            }

            [HarmonyPriority(int.MinValue)]
            public static void Postfix(ITab_Bills __instance, List<RecipeDef> __state)
            {
                var table = __instance.SelTable;
                table.def.allRecipesCached = __state;
            }
        }

        [HarmonyPatch(typeof(Bill_Production), "ExposeData")]
        public static class Bill_Production_ExposeData_Patch
        {
            public static void Postfix(Bill_Production __instance)
            {
                LongEventHandler.toExecuteWhenFinished.Add(delegate
                {
                    if (__instance.billStack != null && __instance.billStack.billGiver is ThingWithComps thing
                        && thing.IsLinkedTo(VFEM_DefOf.VFEM2_MannequinStand))
                    {
                        __instance.recipe = __instance.recipe.ContractedRecipe();
                    }
                });
            }
        }

        [HarmonyPatch(typeof(BillStack), "AddBill")]
        public static class BillStack_AddBill_Patch
        {
            public static void Postfix(Bill bill)
            {
                if (bill.billStack != null && bill.billStack.billGiver is ThingWithComps thing
                    && thing.IsLinkedTo(VFEM_DefOf.VFEM2_MannequinStand))
                {
                    bill.recipe = bill.recipe.ContractedRecipe();
                }
            }
        }
        [HarmonyPatch(typeof(Bill_Production), "Clone")]
        public static class Bill_Production_Clone_Patch
        {
            public static void Postfix(Bill __result)
            {
                if (__result is Bill_Production bill && bill.billStack != null
                    && bill.billStack.billGiver is ThingWithComps thing
                    && thing.IsLinkedTo(VFEM_DefOf.VFEM2_MannequinStand))
                {
                    bill.recipe = bill.recipe.ContractedRecipe();
                }
            }
        }

        private static HashSet<RecipeDef> contractedRecipes = new HashSet<RecipeDef>();

        public static RecipeDef ContractedRecipe(this RecipeDef recipe)
        {
            if (contractedRecipes.Contains(recipe) || recipe is null)
            {
                return recipe;
            }
            var contractedRecipe = recipe.Clone() as RecipeDef;
            contractedRecipe.ingredients = contractedRecipe.ingredients.Select(x => x.Clone() as IngredientCount).ToList();
            foreach (var ingredient in contractedRecipe.ingredients)
            {
                ingredient.count = Mathf.CeilToInt(ingredient.count * 0.9f);
            }
            contractedRecipes.Add(contractedRecipe);
            return contractedRecipe;
        }

        public static object Clone(this object obj)
        {
            var cloneMethod = obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return cloneMethod.Invoke(obj, null);
        }
    }
}
