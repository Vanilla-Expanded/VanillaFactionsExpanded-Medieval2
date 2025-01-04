
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class LeatherTerrainTemplateDef : Def
    {
        [NoTranslate]
        public string texturePath;

        [NoTranslate]
        public List<string> tags;

        public List<ResearchProjectDef> researchPrerequisites;

        public TerrainDef burnedDef;

        public int costList;

        public DesignatorDropdownGroupDef designatorDropdown;

        public List<StatModifier> statBases;

        public int uiOrder;

        public int renderPrecedenceStart;

        public int constructionSkillPrerequisite;

        public bool canGenerateDefaultDesignator = true;

        public StyleCategoryDef dominantStyleCategory;

        public List<ThingCategoryDef> thingCategories;
    }
}