using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public class WarbowArrow : Bullet
    {
        private Vector3 LookTowards =>
    new(this.destination.x - this.origin.x, this.def.Altitude, this.destination.z - this.origin.z +
                                                               this.ArcHeightFactor * (4 - 8 * this.DistanceCoveredFraction));


        public override Quaternion ExactRotation => Quaternion.LookRotation(this.LookTowards);

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
            Vector3 drawPos = DrawPos;
            Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
            Graphics.DrawMesh(MeshPool.GridPlane(DrawSize), position, ExactRotation, DrawMat, 0);
            Comps_PostDraw();
        }
    }
}
