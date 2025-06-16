using System;
using Verse;
using Verse.AI;
using RimWorld;
using VFEMedieval;

namespace VFEMedieval
{
    class WorkGiver_TakeHoneyOutOfApiary : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(VFEM_DefOf.VFEM2_Apiary);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_Apiary tNR_Apiary = t as Building_Apiary;
            if (tNR_Apiary == null || !tNR_Apiary.HoneyReady)
            {
                return false;
            }
            int skill = pawn.skills.GetSkill(SkillDefOf.Animals).Level;
            if (skill < 5)
            {
                JobFailReason.Is("VFEM2_BelowAnimalSkill".Translate());
                return false;
            }
           
            if (t.IsBurning())
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(VFEM_DefOf.VFEM2_TakeHoneyOutOfApiary, t);
        }
    }
}
