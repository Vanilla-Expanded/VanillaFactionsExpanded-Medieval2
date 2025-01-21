using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
    public static class MerchantGuildTweenerUtility
    {
        private const float BaseRadius = 0.15f;

        private const float BaseDistToCollide = 0.2f;

        public static Vector3 PatherTweenedPosRoot(MerchantGuild merchantGuild)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            if (!merchantGuild.Spawned)
            {
                return worldGrid.GetTileCenter(merchantGuild.Tile);
            }
            if (merchantGuild.pather.Moving)
            {
                float num = (merchantGuild.pather.IsNextTilePassable() ? (1f - merchantGuild.pather.nextTileCostLeft / merchantGuild.pather.nextTileCostTotal) : 0f);
                int tileID = ((merchantGuild.pather.nextTile != merchantGuild.Tile || merchantGuild.pather.previousTileForDrawingIfInDoubt == -1) ? merchantGuild.Tile : merchantGuild.pather.previousTileForDrawingIfInDoubt);
                return worldGrid.GetTileCenter(merchantGuild.pather.nextTile) * num + worldGrid.GetTileCenter(tileID) * (1f - num);
            }
            return worldGrid.GetTileCenter(merchantGuild.Tile);
        }

        public static Vector3 MerchantGuildCollisionPosOffsetFor(MerchantGuild merchantGuild)
        {
            if (!merchantGuild.Spawned)
            {
                return Vector3.zero;
            }
            bool flag = merchantGuild.Spawned && merchantGuild.pather.Moving;
            float num = 0.15f * Find.WorldGrid.averageTileSize;
            if (!flag || merchantGuild.pather.nextTile == merchantGuild.pather.Destination)
            {
                int num2 = ((!flag) ? merchantGuild.Tile : merchantGuild.pather.nextTile);
                int merchantGuildsCount = 0;
                int merchantGuildsWithLowerIdCount = 0;
                GetMerchantGuildsStandingAtOrAboutToStandAt(num2, out merchantGuildsCount, out merchantGuildsWithLowerIdCount, merchantGuild);
                if (merchantGuildsCount == 0)
                {
                    return Vector3.zero;
                }
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Find.WorldGrid.GetTileCenter(num2), GenGeo.RegularPolygonVertexPosition(merchantGuildsCount, merchantGuildsWithLowerIdCount) * num);
            }
            if (DrawPosCollides(merchantGuild))
            {
                Rand.PushState();
                Rand.Seed = merchantGuild.ID;
                float f = Rand.Range(0f, 360f);
                Rand.PopState();
                Vector2 point = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num;
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(PatherTweenedPosRoot(merchantGuild), point);
            }
            return Vector3.zero;
        }

        private static void GetMerchantGuildsStandingAtOrAboutToStandAt(int tile, out int merchantGuildsCount, out int merchantGuildsWithLowerIdCount, MerchantGuild forMerchantGuild)
        {
            merchantGuildsCount = 0;
            merchantGuildsWithLowerIdCount = 0;
            List<MerchantGuild> merchantGuilds = Find.WorldObjects.AllWorldObjects.OfType<MerchantGuild>().ToList();
            for (int i = 0; i < merchantGuilds.Count; i++)
            {
                MerchantGuild merchantGuild = merchantGuilds[i];
                if (merchantGuild.Tile != tile)
                {
                    if (!merchantGuild.pather.Moving || merchantGuild.pather.nextTile != merchantGuild.pather.Destination || merchantGuild.pather.Destination != tile)
                    {
                        continue;
                    }
                }
                else if (merchantGuild.pather.Moving)
                {
                    continue;
                }
                merchantGuildsCount++;
                if (merchantGuild.ID < forMerchantGuild.ID)
                {
                    merchantGuildsWithLowerIdCount++;
                }
            }
        }

        private static bool DrawPosCollides(MerchantGuild merchantGuild)
        {
            Vector3 a = PatherTweenedPosRoot(merchantGuild);
            float num = Find.WorldGrid.averageTileSize * 0.2f;
            List<MerchantGuild> merchantGuilds = Find.WorldObjects.AllWorldObjects.OfType<MerchantGuild>().ToList();
            for (int i = 0; i < merchantGuilds.Count; i++)
            {
                MerchantGuild merchantGuild2 = merchantGuilds[i];
                if (merchantGuild2 != merchantGuild && Vector3.Distance(a, PatherTweenedPosRoot(merchantGuild2)) < num)
                {
                    return true;
                }
            }
            return false;
        }
    }
}