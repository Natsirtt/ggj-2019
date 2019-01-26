using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldGenerationParameters", menuName = "World/Generation Parameters")]
public class WorldGenerationParameters : ScriptableObject
{
    [Serializable] public struct Grid
    {
        public Vector2Int Size;
    }
    
    [Serializable] public struct Resources
    {
        public int startingWoodAmount;
        public int woodPerTree;
        public int expeditionWoodCostPerTile;
    }

    [Serializable] public struct Infrastructures
    {
        public Vector2Int hearthMinDistanceFromMapEdge;
    }

    [Serializable] public struct Forests
    {
        public Vector2Int numberOfPathsRange;
        public Vector2Int patchesPerPathRange;
        public Vector2Int woodAmountRangePerPatch;
        [Tooltip("This number is the distance that is removed from the critical distance. The generation algorithm will create a new forest patch at the furthest reachable point according to the thoretical amount of wood available, then the patch will be brought back by the number of tiles provided here. Lower is harder, 0 is critical path.")]
        public int patchesDifficultyDistanceModifier;
        //public int minDistanceFromPathLastSeed;
        //public int maxDistanceFromPathLastSeed;
        public int minPatchEuclidianRadius;
        public int maxPatchEuclidianRadius;
        public Vector2 patchDensityRange;
    }

    public Grid grid;
    public Resources resources;
    public Infrastructures infrastructures;
    public Forests forests;
}
