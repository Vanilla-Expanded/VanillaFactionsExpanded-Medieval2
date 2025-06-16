using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;

namespace VFEMedieval
{


    public class CompCureHypothermia : ThingComp
    {
        public CompProperties_CureHypothermia Props
        {
            get
            {
                return (CompProperties_CureHypothermia)this.props;
            }
        }

        public override void CompTickRare()
        {
            Building_Bed parentAsBed = (Building_Bed)this.parent;
            if (parentAsBed != null)
            {
                Pawn pawn = parentAsBed.GetCurOccupantAt(parentAsBed.Position);
                if (pawn?.health?.hediffSet?.hediffs.Find(h => h.def.defName == "Hypothermia") is Hediff hediff && hediff != null)
                {
                    hediff.Severity -= 0.01f;
                }
            }
        }
    }
}
