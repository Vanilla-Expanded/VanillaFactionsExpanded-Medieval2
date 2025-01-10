
using System.Collections.Generic;
using RimWorld;
using Verse;
using System;

namespace VFEMedieval
{
    public class CobblestoneWallTemplateDef : Def
    {

        public string texPath;
        public Type graphicClass;
        public LinkDrawerType linkType;

        public LinkFlags linkFlags;
        public DamageGraphicData damageData;

        public ThingCategory category;

        public Type thingClass;

        public SoundDef soundImpactDefault;

        public DrawerType drawerType;

        public EffecterDef repairEffect;

        public ThingDef filthLeaving;

        [NoTranslate]
        public string uiIconPath;



        public BuildingProperties building;

        public bool leaveResourcesWhenKilled;

        public AltitudeLayer altitudeLayer;

        public Traversability passability;

        public bool blockWind;

        public bool castEdgeShadows;
        public float fillPercent;
        public bool coversFloor;
        public int placingDraggableDimensions;
        public TickerType tickerType;
        public bool rotatable;
        public bool selectable;
        public bool neverMultiSelect;
        public bool useStuffTerrainAffordance;
        public TerrainAffordanceDef terrainAffordanceNeeded;
        public bool holdsRoof;
        public DesignationCategoryDef designationCategory;
        public float staticSunShadowHeight;
        public bool blockLight;

        public float fertility;

        public List<StuffCategoryDef> stuffCategories;
        public List<CompProperties> comps;
        public List<DamageMultiplier> damageMultipliers;
        public DesignatorDropdownGroupDef designatorDropdown;




    }
}