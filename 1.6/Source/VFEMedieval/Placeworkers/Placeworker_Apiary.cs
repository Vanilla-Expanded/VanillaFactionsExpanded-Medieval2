using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    class Placeworker_Apiary : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(3))
            {
                List<Thing> list = map.thingGrid.ThingsListAt(c);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing2 = list[i];
                    if (thing2 != thingToIgnore && ((thing2.def.category == ThingCategory.Building && thing2.def == VFEM_DefOf.VFEM2_Apiary) || ((thing2.def.IsBlueprint || thing2.def.IsFrame) && thing2.def.entityDefToBuild is ThingDef && ((ThingDef)thing2.def.entityDefToBuild) ==VFEM_DefOf.VFEM2_Apiary)))
                    {
                        return "VFEM2_PlaceWorker".Translate();
                    }
                }
            }
            if (center.Roofed(map)) return "VFEM2_PlaceWorkerNoRoof".Translate();
            return true;
        }
    }
}
