using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


namespace VFEMedieval
{


    /*

        [HarmonyPatch(typeof(GenRecipe))]
        [HarmonyPatch("MakeRecipeProducts")]

        public static class VFEMedieval_GenRecipe_MakeRecipeProducts_Patch
        {

            public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
            {
                List<Thing> resultingList = values.ToList();
                Building_WorkTable workbench;
                if ((workbench = billGiver as Building_WorkTable) != null && recipeDef.products != null)
                {
                    if (workbench.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Contains(ANVIL_HERE) == true)
                    {

                        foreach (Thing thing in resultingList)
                        {
                            CompQuality compQuality = thing.TryGetComp<CompQuality>();
                            if (compQuality?.Quality < QualityCategory.Normal)
                            {
                                if (Rand.Chance(0.2f))
                                {
                                    compQuality.SetQuality(compQuality.Quality + 1, null);
                                    Messages.Message("VFEM2_ItemImproved".Translate(thing.LabelCap, compQuality.Quality.ToString()), worker, MessageTypeDefOf.PositiveEvent, null, historical: false);
                                }

                            }

                        }
                    }






                }

                return resultingList;









            }

        }

        */



}
