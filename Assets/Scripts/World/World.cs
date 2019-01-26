using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class World : MonoBehaviour
{
    // Utils

    private static World worldInstance = null;
    public static World Get()
    {
        if (worldInstance == null)
        {
            worldInstance = FindObjectOfType<World>();
            if (worldInstance == null)
            {
                var newWorld = new GameObject("World");
                worldInstance = newWorld.AddComponent<World>();
            }
        }
        return worldInstance;
    }

    // Internal types

    public struct Tile
    {
        public enum Type
        {
            Grass,
            Snow,
            Mountain,
            Campfire,
            Hearth
        }

        public Vector2Int Coordinates { get; private set; }
        public Type TileType { get; set; }

        public Tile(Vector2Int coordinates, Type type)
        {
            Coordinates = coordinates;
            TileType = type;
        }
    }

    public Dictionary<Vector2Int, Tile> Tiles { get; private set; }
    public Inventory GlobalInventory { get; private set; }
    public Vector2Int GridSize { get; private set; }

    [SerializeField]
    private WorldGenerationParameters generationParameters;

    // Leave the seed to 0 for using the current time. Provide a hardcoded seed otherwise.
    [SerializeField]
    private int seed = 0;

    void Start()
    {
        if (seed == 0)
        {
            // TODO TickCount is not the best to use
            GenerateWorld(Environment.TickCount, generationParameters);
        }
        else
        {
            GenerateWorld(seed, generationParameters);
        }
    }

    void Awake()
    {
        GlobalInventory = gameObject.AddComponent<Inventory>();
    }

    void GenerateWorld(int seed, WorldGenerationParameters parameters)
    {
        Tiles.Clear();
        Random.InitState(seed);
        GridSize = parameters.worldGridSize;

        // Creating hearth
        Vector2Int maxHearthGridPosition = parameters.worldGridSize - parameters.hearthMinDistanceFromMapEdge;
        var hearthGridPos = new Vector2Int(Random.Range(-maxHearthGridPosition.x, maxHearthGridPosition.x), Random.Range(-maxHearthGridPosition.y, maxHearthGridPosition.y));
        Tiles.Add(hearthGridPos, new Tile(hearthGridPos, Tile.Type.Hearth));

        // Seeding woods
    }
}
