using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using VFEMedieval;

namespace VFEMedieval
{
    public class Building_Apiary : Building
    {
        int ApiaryProgress;
        int duration = 420000;
        public int tickBeforeTend = 120000;
        public float neededFlower;
        float progressPercentage;
        private int flowerCount;
        private int flowerNeeded;
        private List<IntVec3> cellsAround = new List<IntVec3>();

        public const int minFlowersNeeded =28;

        public bool HoneyReady
        {
            get
            {
                return ApiaryProgress >= duration;
            }
        }

        public bool needTend
        {
            get
            {
                return tickBeforeTend <= 0;
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return this.duration - this.ApiaryProgress;
            }
        }

        private bool IsthereFlowerAround;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ApiaryProgress, "ApiaryProgress", 0, false);
            Scribe_Values.Look<int>(ref this.tickBeforeTend, "tickBeforeTend", 0, false);
            Scribe_Values.Look<int>(ref this.flowerCount, "flowerCount", 0, false);
            Scribe_Values.Look<float>(ref this.neededFlower, "neededFlower", 0, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            progressPercentage = (float)this.ApiaryProgress / this.duration;
            IsthereFlowerAround = FlowerAround();
            flowerNeeded = FlowerNeeded();

            if (!(this.AmbientTemperature < 10) && IsthereFlowerAround)
            {
                this.tickBeforeTend -= 250;
                if (!this.needTend)
                {
                    this.ApiaryProgress += 250;
                }
                else
                {
                    this.ApiaryProgress += 125;
                }
            }
        }

        private void Reset()
        {
            this.ApiaryProgress = 0;
        }

        public void ResetTend(Pawn pawn)
        {
            SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Animals);
            if (Rand.Range(0, 21 - skill.Level) == 1)
            {
                this.tickBeforeTend = 120000;
            }
            else
            {
                pawn.health.AddHediff(VFEM_DefOf.VFEM2_Sting);
                pawn.needs.mood.thoughts.memories.TryGainMemoryFast(VFEM_DefOf.VFEM2_StingMoodDebuff);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "VFEM2_TendingFailed".Translate(), 5f);
            }
            skill.Learn(100);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            if (this.HoneyReady)
            {
                stringBuilder.AppendLine("VFEM2_Ready".Translate());
            }
            else
            {
                if (!IsthereFlowerAround)
                {
                    stringBuilder.AppendLine("VFEM2_NeedFlower".Translate(flowerNeeded));
                }
                else
                {
                    if (this.AmbientTemperature < 10)
                    {
                        stringBuilder.AppendLine("VFEM2_Resting".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLine("FermentationProgress".Translate(this.progressPercentage.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod()));
                        if (this.needTend)
                        {
                            stringBuilder.AppendLine("VFEM2_NeedTend".Translate());
                        }
                        else
                        {
                            stringBuilder.AppendLine("VFEM2_NeedTIn".Translate(this.tickBeforeTend.ToStringTicksToPeriod()));
                        }
                    }
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutHoney()
        {
            if (!this.HoneyReady)
            {
                Log.Warning("Tried to get honey but it's not yet ready.");
                return null;
            }
            Thing thing = ThingMaker.MakeThing(VFEM_DefOf.VFEM2_Honey, null);
            thing.stackCount = flowerCount>75 ? 75: flowerCount;
            this.Reset();
            return thing;
        }

        private int FlowerNeeded()
        {
            int i = minFlowersNeeded;
            i -= flowerCount;
            return i;
        }

        public bool FlowerAround()
        {
            flowerCount = 0;
            cellsAround = CellsAroundA(this.TrueCenter().ToIntVec3(), this.Map);
            foreach (IntVec3 cell in cellsAround)
            {
                foreach (Thing item in cell.GetThingList(this.Map))
                {
                    if (item.def.plant != null && (item.def.plant.purpose == PlantPurpose.Beauty || item.def.defName == "VCE_Blueberry"))
                    {
                        flowerCount++;
                    }
                }
            }
            if (flowerCount >= minFlowersNeeded)
            {
                return true;
            }
            return false;
        }

        public List<IntVec3> CellsAroundA(IntVec3 pos, Map map)
        {
            List<IntVec3> cellsAround = new List<IntVec3>();
            if (!pos.InBounds(map))
            {
                return cellsAround;
            }
            IEnumerable<IntVec3> cells = CellRect.CenteredOn(this.Position, 10).Cells;
            foreach (IntVec3 item in cells)
            {
                if (item.InHorDistOf(pos, 9.9f))
                {
                    cellsAround.Add(item);
                }
            }
            return cellsAround;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to max",
                    action = delegate ()
                    {
                        this.ApiaryProgress = this.duration;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Add progress",
                    action = delegate ()
                    {
                        this.ApiaryProgress += 12000;
                        this.tickBeforeTend -= 12000;
                    }
                };
            }
            yield break;
        }
    }
}
