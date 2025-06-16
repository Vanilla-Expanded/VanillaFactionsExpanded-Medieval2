
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_ReduceRest : CompUseEffect
    {
        public CompProperties_UseEffectReduceRest Props => (CompProperties_UseEffectReduceRest)props;

        public override void DoEffect(Pawn user)
        {
            Need_Rest need_Rest = user.needs.TryGetNeed<Need_Rest>();
            if (need_Rest != null) {

                float realDecrease = need_Rest.MaxLevel * Props.amount;
                need_Rest.CurLevel -= realDecrease;
            }
          

        }


    }
}
