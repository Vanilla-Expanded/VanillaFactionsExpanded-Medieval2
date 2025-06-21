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
    
        public static Dictionary<Pawn, float> pawnLearningFactorSinglePassionMultiplier = new Dictionary<Pawn, float>();
        public static Dictionary<Pawn, float> pawnLearningFactorDoublePassionMultiplier = new Dictionary<Pawn, float>();


        public static void AddPawnMoodTimeMultiplierToList(Pawn thing, float modifier)
        {        
                pawnMoodTimeMultiplier[thing] = modifier;   
        }

        public static void RemovePawnMoodTimeMultiplierFromList(Pawn thing)
        {
            pawnMoodTimeMultiplier.Remove(thing);
        }

     

        public static void AddPawnLearningFactorSinglePassionMultiplierToList(Pawn thing, float modifier)
        {
            pawnLearningFactorSinglePassionMultiplier[thing] = modifier;
        }

        public static void RemovePawnLearningFactorSinglePassionMultiplierFromList(Pawn thing)
        {
            pawnLearningFactorSinglePassionMultiplier.Remove(thing);
        }

        public static void AddPawnLearningFactorDoublePassionMultiplierToList(Pawn thing, float modifier)
        {
            pawnLearningFactorDoublePassionMultiplier[thing] = modifier;
        }

        public static void RemovePawnLearningFactorDoublePassionMultiplierFromList(Pawn thing)
        {
            pawnLearningFactorDoublePassionMultiplier.Remove(thing);
        }

    }
}
