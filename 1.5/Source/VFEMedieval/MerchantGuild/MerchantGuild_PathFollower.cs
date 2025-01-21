using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace VFEMedieval
{

    public class MerchantGuild_PathFollower : IExposable
    {
        private MerchantGuild merchantGuild;

        private bool moving;

        private bool paused;

        public int nextTile = -1;

        public int previousTileForDrawingIfInDoubt = -1;

        public float nextTileCostLeft;

        public float nextTileCostTotal = 1f;

        private int destTile;

        public WorldPath curPath;

        public int lastPathedTargetTile;

        public const int MaxMoveTicks = 30000;

        private const int MaxCheckAheadNodes = 20;

        private const int MinCostWalk = 50;

        private const int MinCostAmble = 60;

        public const float DefaultPathCostToPayPerTick = 1f;

        public const int FinalNoRestPushMaxDurationTicks = 10000;

        public int Destination => destTile;

        public bool Moving
        {
            get
            {
                if (moving)
                {
                    return merchantGuild.Spawned;
                }
                return false;
            }
        }

        public bool MovingNow
        {
            get
            {
                if (Moving && !Paused)
                {
                    return true;
                }
                return false;
            }
        }

        public bool Paused
        {
            get
            {
                if (Moving)
                {
                    return paused;
                }
                return false;
            }
            set
            {
                if (value != paused)
                {
                    if (!value)
                    {
                        paused = false;
                    }
                    else if (!Moving)
                    {
                        Log.Error("Tried to pause merchantGuild movement of " + merchantGuild.ToStringSafe() + " but it's not moving.");
                    }
                    else
                    {
                        paused = true;
                    }
                }
            }
        }

        public MerchantGuild_PathFollower(MerchantGuild merchantGuild)
        {
            this.merchantGuild = merchantGuild;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref moving, "moving", defaultValue: true);
            Scribe_Values.Look(ref paused, "paused", defaultValue: false);
            Scribe_Values.Look(ref nextTile, "nextTile", 0);
            Scribe_Values.Look(ref previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0);
            Scribe_Values.Look(ref nextTileCostLeft, "nextTileCostLeft", 0f);
            Scribe_Values.Look(ref nextTileCostTotal, "nextTileCostTotal", 0f);
            Scribe_Values.Look(ref destTile, "destTile", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != 0 && moving 
                && !StartPath(destTile, repathImmediately: true, resetPauseStatus: false))
            {
                StopDead();
            }
        }

        public bool StartPath(int destTile, bool repathImmediately = false, bool resetPauseStatus = true)
        {
            Find.WorldPathPool.paths.Clear();
            if (resetPauseStatus)
            {
                paused = false;
            }

            if (!IsPassable(merchantGuild.Tile) && !TryRecoverFromUnwalkablePosition())
            {
                return false;
            }
            if (moving && curPath != null && this.destTile == destTile)
            {
                return true;
            }
            if (!merchantGuild.CanReach(destTile))
            {
                PatherFailed();
                return false;
            }
            this.destTile = destTile;
            if (nextTile < 0 || !IsNextTilePassable())
            {
                nextTile = merchantGuild.Tile;
                nextTileCostLeft = 0f;
                previousTileForDrawingIfInDoubt = -1;
            }
            if (AtDestinationPosition())
            {
                PatherArrived();
                return true;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = true;
            if (repathImmediately && TrySetNewPath() && nextTileCostLeft <= 0f && moving)
            {
                TryEnterNextPathTile();
            }
            return true;
        }

        public void StopDead()
        {
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = false;
            paused = false;
            nextTile = merchantGuild.Tile;
            previousTileForDrawingIfInDoubt = -1;
            nextTileCostLeft = 0f;
        }

        public void PatherTick()
        {
            if (!paused)
            {
                if (nextTileCostLeft > 0f)
                {
                    nextTileCostLeft -= CostToPayThisTick();
                }
                else if (moving)
                {
                    TryEnterNextPathTile();
                }
            }
        }

        public void Notify_Teleported_Int()
        {
            StopDead();
        }

        private bool IsPassable(int tile)
        {
            return !Find.World.Impassable(tile);
        }

        public bool IsNextTilePassable()
        {
            return IsPassable(nextTile);
        }

        private bool TryRecoverFromUnwalkablePosition()
        {
            if (GenWorldClosest.TryFindClosestTile(merchantGuild.Tile, (int t) => IsPassable(t), out var foundTile))
            {
                Log.Warning(string.Concat(merchantGuild, " on unwalkable tile ", merchantGuild.Tile, ". Teleporting to ", foundTile));
                merchantGuild.Tile = foundTile;
                return true;
            }
            Log.Error(string.Concat(merchantGuild, " on unwalkable tile ", merchantGuild.Tile, ". Could not find walkable position nearby. Removed."));
            merchantGuild.Destroy();
            return false;
        }

        private void PatherArrived()
        {
            StopDead();
        }

        private void PatherFailed()
        {
            StopDead();
        }

        private void TryEnterNextPathTile()
        {
            if (!IsNextTilePassable())
            {
                PatherFailed();
                return;
            }
            merchantGuild.Tile = nextTile;
            if (!NeedNewPath() || TrySetNewPath())
            {
                if (AtDestinationPosition())
                {
                    PatherArrived();
                }
                else if (curPath.NodesLeftCount == 0)
                {
                    Log.Error(string.Concat(merchantGuild, " ran out of path nodes. Force-arriving."));
                    PatherArrived();
                }
                else
                {
                    SetupMoveIntoNextTile();
                }
            }
        }

        private void SetupMoveIntoNextTile()
        {
            if (curPath.NodesLeftCount < 2)
            {
                Log.Error(string.Concat(merchantGuild, " at ", merchantGuild.Tile, " ran out of path nodes while pathing to ", destTile, "."));
                PatherFailed();
                return;
            }
            nextTile = curPath.ConsumeNextNode();
            previousTileForDrawingIfInDoubt = -1;
            if (Find.World.Impassable(nextTile))
            {
                Log.Error(string.Concat(merchantGuild, " entering ", nextTile, " which is unwalkable."));
            }
            int num = CostToMove(merchantGuild.Tile, nextTile);
            nextTileCostTotal = num;
            nextTileCostLeft = num;
        }

        private int CostToMove(int start, int end)
        {
            return CostToMove(merchantGuild, start, end);
        }

        public static int CostToMove(MerchantGuild merchantGuild, int start, int end, int? ticksAbs = null)
        {
            return CostToMove(merchantGuild.TicksPerMove, start, end, ticksAbs, perceivedStatic: false, null, 
                null, false);
        }

        public static int CostToMove(int merchantGuildTicksPerMove, int start, int end, int? ticksAbs = null, bool perceivedStatic = false, StringBuilder explanation = null, string merchantGuildTicksPerMoveExplanation = null, bool immobile = false)
        {
            if (start == end)
            {
                return 0;
            }
            if (explanation != null)
            {
                explanation.Append(merchantGuildTicksPerMoveExplanation);
                explanation.AppendLine();
            }
            StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
            float num = ((!perceivedStatic || explanation != null) ? WorldPathGrid.CalculatedMovementDifficultyAt(end, perceivedStatic, ticksAbs, stringBuilder) : Find.WorldPathGrid.PerceivedMovementDifficultyAt(end));
            float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(start, end, stringBuilder);
            if (explanation != null && !immobile)
            {
                explanation.AppendLine();
                explanation.Append("TileMovementDifficulty".Translate() + ":");
                explanation.AppendLine();
                explanation.Append(stringBuilder.ToString().Indented("  "));
                explanation.AppendLine();
                explanation.Append("  = " + (num * roadMovementDifficultyMultiplier).ToString("0.#"));
            }
            int value = (int)((float)merchantGuildTicksPerMove * num * roadMovementDifficultyMultiplier);
            value = Mathf.Clamp(value, 1, 30000);
            if (explanation != null)
            {
                explanation.AppendLine();
                if (immobile)
                {
                    explanation.Append("EncumberedMerchantGuildTilesPerDayTip".Translate());
                }
                else
                {
                    explanation.AppendLine();
                    explanation.Append("FinalMerchantGuildMovementSpeed".Translate() + ":");
                    int num2 = Mathf.CeilToInt((float)value / 1f);
                    explanation.AppendLine();
                    explanation.Append("  " + (60000f / (float)merchantGuildTicksPerMove).ToString("0.#") + " / " + (num * roadMovementDifficultyMultiplier).ToString("0.#") + " = " + (60000f / (float)num2).ToString("0.#") + " " + "TilesPerDay".Translate());
                }
            }
            return value;
        }

        public static bool IsValidFinalPushDestination(int tile)
        {
            List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < allWorldObjects.Count; i++)
            {
                if (allWorldObjects[i].Tile == tile && !(allWorldObjects[i] is MerchantGuild))
                {
                    return true;
                }
            }
            return false;
        }

        private float CostToPayThisTick()
        {
            float num = 1f;
            if (num < nextTileCostTotal / 30000f)
            {
                num = nextTileCostTotal / 30000f;
            }
            return num;
        }

        private bool TrySetNewPath()
        {
            WorldPath worldPath = GenerateNewPath();
            if (!worldPath.Found)
            {
                PatherFailed();
                return false;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = worldPath;
            return true;
        }

        private WorldPath GenerateNewPath()
        {
            int num = ((moving && nextTile >= 0 && IsNextTilePassable()) ? nextTile : merchantGuild.Tile);
            lastPathedTargetTile = destTile;
            WorldPath worldPath = Find.WorldPathFinder.FindPath(num, destTile, null);
            if (worldPath.Found && num != merchantGuild.Tile)
            {
                if (worldPath.NodesLeftCount >= 2 && worldPath.Peek(1) == merchantGuild.Tile)
                {
                    worldPath.ConsumeNextNode();
                    if (moving)
                    {
                        previousTileForDrawingIfInDoubt = nextTile;
                        nextTile = merchantGuild.Tile;
                        nextTileCostLeft = nextTileCostTotal - nextTileCostLeft;
                    }
                }
                else
                {
                    worldPath.AddNodeAtStart(merchantGuild.Tile);
                }
            }
            return worldPath;
        }

        private bool AtDestinationPosition()
        {
            return merchantGuild.Tile == destTile;
        }

        private bool NeedNewPath()
        {
            if (!moving)
            {
                return false;
            }
            if (curPath == null || !curPath.Found || curPath.NodesLeftCount == 0)
            {
                return true;
            }
            for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
            {
                int tileID = curPath.Peek(i);
                if (Find.World.Impassable(tileID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}