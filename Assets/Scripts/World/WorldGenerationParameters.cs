using UnityEngine;

[CreateAssetMenu(fileName = "WorldGenerationParameters", menuName = "World/Generation Parameters")]
public class WorldGenerationParameters : ScriptableObject
{
    public int startingWood;
    public int minWoodDistance;
    public int maxWoodDistance;
    public int minWoodRadius;
    public int maxWoodRadius;
}
