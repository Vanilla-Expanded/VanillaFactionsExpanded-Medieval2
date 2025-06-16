using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class CompIngestedThoughtFromQuality : ThingComp
    {
        private CompProperties_IngestedThoughtFromQuality Props => (CompProperties_IngestedThoughtFromQuality)props;

        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            if (ingester.needs.mood != null)
            {
            
                int quality = (int)parent.GetComp<CompQuality>().Quality;

            
                ingester.needs.mood.thoughts.memories.TryGainMemory(Props.ingestedThoughts[quality]);

            }
        }
    }
}