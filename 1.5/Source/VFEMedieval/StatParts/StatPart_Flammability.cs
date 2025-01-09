
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace VFEMedieval
{
    public class StatPart_Flammability : StatPart
    {
   

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
               
                if (req.Thing.def == VFEM_DefOf.VFEM2_TimberedWall)
                {
                    val += 0.5f;
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                if (req.Thing.def == VFEM_DefOf.VFEM2_TimberedWall)
                {
                    return "VFEM2_StatsReport_Flammability".Translate();
                }
            }
            return null;
        }

      
    }
}