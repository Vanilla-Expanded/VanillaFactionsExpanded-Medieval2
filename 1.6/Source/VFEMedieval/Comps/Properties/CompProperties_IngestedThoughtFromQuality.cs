using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_IngestedThoughtFromQuality : CompProperties
    {
        public List<ThoughtDef> ingestedThoughts;

        public CompProperties_IngestedThoughtFromQuality()
        {
            compClass = typeof(CompIngestedThoughtFromQuality);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            // Parent def does not have CompQuality
            if (!parentDef.HasComp(typeof(CompQuality)))
                yield return $"{parentDef} does not have CompQuality.";

            
        }
    }
}