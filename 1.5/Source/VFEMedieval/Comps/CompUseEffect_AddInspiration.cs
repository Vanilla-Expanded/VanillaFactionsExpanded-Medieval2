
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class CompUseEffect_AddInspiration : CompUseEffect
    {
        public CompProperties_UseEffectAddInspiration Props => (CompProperties_UseEffectAddInspiration)props;

        public override void DoEffect(Pawn user)
        {
            if (Rand.Chance(Props.chance))
            {
                InspirationDef randomAvailableInspirationDef = user.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
                user.mindState.inspirationHandler.TryStartInspiration(randomAvailableInspirationDef);
            }
            


        }


    }
}
