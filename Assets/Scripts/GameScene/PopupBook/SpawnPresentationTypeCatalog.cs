using System.Collections.Generic;

namespace GameScene.PopupBook
{
    public static class SpawnPresentationTypeCatalog
    {
        private static readonly HashSet<string> BuildingTypes = new HashSet<string>
        {
            "BubbleGenerator",
            "Crater",
            "ElectricTower",
            "FrenzyTotem",
            "GroundCannon",
            "GroundTower",
            "HealingTotem",
            "LifeTree",
            "ManaWell",
            "PveNatureSlimeNest",
            "PveVineColony",
            "PveWaterSlimeNest",
            "RallyingTorch",
            "RockTurret",
            "Vine",
            "VineColony",
            "WindTotem"
        };

        public static bool IsBuilding(string objectType)
        {
            return !string.IsNullOrEmpty(objectType) && BuildingTypes.Contains(objectType);
        }
    }
}
