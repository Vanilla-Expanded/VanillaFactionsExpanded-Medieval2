
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_GiveThought : CompUseEffect
    {
        public CompProperties_UseEffectGiveThought Props => (CompProperties_UseEffectGiveThought)props;

        public override void DoEffect(Pawn user)
        {

            user.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.thought);


        }


    }
}
