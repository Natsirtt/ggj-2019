using UnityEngine;

[CreateAssetMenu(fileName = "WorldGenerationParameters", menuName = "World/Generation Parameters")]
public class WorldGenerationParameters : ScriptableObject
{
    public Vector2Int worldGridSize;
    public Vector2Int hearthMinDistanceFromMapEdge;
    public int startingWood;
    public int minWoodDistanceFromPathLastSeed;
    public int maxWoodDistanceFromPathLastSeed;
    public int minWoodRadiusFromLastPathSeed;
    public int maxWoodRadiusFromLastPathSeed;
    public float nextSeedInWoodPathAngle;
}
