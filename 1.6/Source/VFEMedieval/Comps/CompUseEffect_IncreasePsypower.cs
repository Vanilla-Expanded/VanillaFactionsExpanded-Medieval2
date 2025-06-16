
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_IncreasePsypower : CompUseEffect
    {
        public CompProperties_UseEffectIncreasePsypower Props => (CompProperties_UseEffectIncreasePsypower)props;

        public override void DoEffect(Pawn user)
        {
            
            if (user.psychicEntropy!=null)
            {
               
                user.psychicEntropy.OffsetPsyfocusDirectly(Props.amount);
             
            }


        }


    }
}
