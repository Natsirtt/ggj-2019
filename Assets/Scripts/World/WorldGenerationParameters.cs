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
        public int expeditionWoodCost;
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
        public int minDistanceFromPathLastSeed;
        public int maxDistanceFromPathLastSeed;
        public int minPatchEuclidianRadius;
        public int maxPatchEuclidianRadius;
        public Vector2 patchDensityRange;
        public float angleForNextSeedInPath;
    }

    public Grid grid;
    public Resources resources;
    public Infrastructures infrastructures;
    public Forests forests;
}
