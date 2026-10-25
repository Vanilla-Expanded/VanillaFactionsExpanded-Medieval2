using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [HarmonyPatch(typeof(GenThing), "ItemCenterAt")]
    public static class GenThing_ItemCenterAt_Patch
    {
        public static bool Prefix(Thing thing, ref Vector3 __result)
        {
            if (!thing.Spawned || thing.Position.GetEdifice(thing.Map)?.def != VFEM_DefOf.VFEM2_AilmentsShelf)
                return true;

            int itemCount = 0;
            int itemIndex = 0;
            var things = thing.Position.GetThingList(thing.Map);
            for (int i = 0; i < things.Count; i++)
            {
                var item = things[i];
                if (item.def.category != ThingCategory.Item)
                    continue;

                itemCount++;
                if (item.thingIDNumber < thing.thingIDNumber)
                    itemIndex++;
            }

            if (itemCount <= 1 || itemCount > 4)
                return true;

            var center = thing.Position.ToVector3Shifted();
            __result = new Vector3(
                center.x + (itemIndex % 2 == 0 ? -0.25f : 0.25f),
                thing.def.Altitude + itemIndex * Altitudes.AltInc / 10f,
                center.z + (itemIndex / 2 == 0 ? -0.25f : 0.25f));
            return false;
        }
    }
}
