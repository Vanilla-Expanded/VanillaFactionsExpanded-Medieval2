using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.UIElements;
using Verse.Noise;
using System.Linq;
using RimWorld.Planet;
using System.Collections;

namespace VFEMedieval
{
    public class WorldComponent_HeraldryPawns : WorldComponent
    {

        public HashSet<Pawn> heraldry_pawns = new HashSet<Pawn>();


        public static WorldComponent_HeraldryPawns Instance;

        public WorldComponent_HeraldryPawns(World world) : base(world) => Instance = this;



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref heraldry_pawns, "heraldry_pawns", LookMode.Reference);


        }

        public void AddHeraldryPawn(Pawn pawn)
        {
            if (pawn != null && !heraldry_pawns.Contains(pawn))
            {
                heraldry_pawns.Add(pawn);
            }

        }

        public void RemoveHeraldryPawn(Pawn pawn)
        {
            if (pawn != null && heraldry_pawns.Contains(pawn))
            {
                heraldry_pawns.Remove(pawn);
            }

        }

    }


}
