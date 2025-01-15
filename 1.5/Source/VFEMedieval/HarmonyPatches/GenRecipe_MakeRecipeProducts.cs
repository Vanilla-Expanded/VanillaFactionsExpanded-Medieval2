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




    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]

    public static class VFEMedieval_GenRecipe_MakeRecipeProducts_Patch
    {

        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
        {
            List<Thing> resultingList = values.ToList();
            Building_WorkTable workbench;
            CompAffectedByFacilities comp;
            if ((workbench = billGiver as Building_WorkTable) != null && recipeDef.products != null)
            {
                if ((comp = workbench.TryGetComp<CompAffectedByFacilities>()) != null)
                {

                    if (comp.LinkedFacilitiesListForReading.ContainsAny(x => x.def == VFEM_DefOf.VFEM2_SmithingAnvil) == true)
                    {
                        foreach (Thing thing in resultingList)
                        {
                            string labelOld = thing.LabelCap;
                            CompQuality compQuality = thing.TryGetComp<CompQuality>();
                            if (compQuality?.Quality < QualityCategory.Normal)
                            {
                                if (Rand.Chance(0.2f))
                                {
                                    compQuality.SetQuality(compQuality.Quality + 1, null);
                                    Messages.Message("VFEM2_ItemImproved_Anvil".Translate(labelOld, compQuality.Quality.ToString()), worker, MessageTypeDefOf.PositiveEvent, null, historical: false);
                                }
                            }
                        }
                    }

                    if (comp.LinkedFacilitiesListForReading.ContainsAny(x => x.def == VFEM_DefOf.VFEM2_StonePolisher) == true)
                    {
                        foreach (Thing thing in resultingList)
                        {
                            string labelOld = thing.LabelCap;
                            CompQuality compQuality = thing.GetInnerIfMinified().TryGetComp<CompQuality>();
                            if (compQuality?.Quality < QualityCategory.Normal)
                            {
                                if (Rand.Chance(0.25f))
                                {
                                    compQuality.SetQuality(compQuality.Quality + 1, null);
                                    Messages.Message("VFEM2_ItemImproved_Polisher".Translate(labelOld, compQuality.Quality.ToString()), worker, MessageTypeDefOf.PositiveEvent, null, historical: false);
                                }
                            }
                        }
                    }

                    if (comp.LinkedFacilitiesListForReading.ContainsAny(x => x.def == VFEM_DefOf.VFEM2_StoneClamp || x.def == VFEM_DefOf.VFEM2_CarvingBoard) == true)
                    {
                        foreach (Thing thing in resultingList)
                        {
                            thing.stackCount= (int)(thing.stackCount*1.1f);
                        }
                    }




                }






            }

            return resultingList;




        }
    }
}







