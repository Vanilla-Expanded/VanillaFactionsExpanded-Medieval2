using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //HarmonyInstance.DEBUG = true;
            VFEMedieval.harmonyInstance.PatchAll();

           
        }
    }

}