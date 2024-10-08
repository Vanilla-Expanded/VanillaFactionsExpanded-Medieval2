using RimWorld;
using Verse;

namespace VFEMedieval
{
    [DefOf]
    public static class VFEM_DefOf
    {

        static VFEM_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VFEM_DefOf));
        }
    }
}