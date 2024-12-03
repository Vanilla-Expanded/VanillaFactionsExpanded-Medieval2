using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace VFEMedieval
{
    public static class StaticCollections
    {      

        public static Dictionary<Pawn,float> pawnMoodTimeMultiplier = new Dictionary<Pawn, float>();

        public static void AddPawnMoodTimeMultiplierToList(Pawn thing, float modifier)
        {        
                pawnMoodTimeMultiplier[thing] = modifier;   
        }

        public static void RemovePawnMoodTimeMultiplierFromList(Pawn thing)
        {
            if (pawnMoodTimeMultiplier.ContainsKey(thing))
            {
                pawnMoodTimeMultiplier.Remove(thing);
            }
        }

    }
}
