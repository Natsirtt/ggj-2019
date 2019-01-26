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
            Fire
        }

        public Vector2 Coordinates { get; private set; }
        public Type TileType { get; set; }

        public Tile(Vector2 coordinates, Type type)
        {
            Coordinates = coordinates;
            TileType = type;
        }
    }

    public Dictionary<Vector2, Tile> Tiles { get; private set; }
    public Inventory GlobalInventory { get; private set; }

    // Leave the seed to 0 for using the current time. Provide a hardcoded seed otherwise.
    [SerializeField]
    private int seed = 0;

    void Start()
    {
        if (seed == 0)
        {
            // TODO TickCount is not the best to use
            GenerateWorld(Environment.TickCount);
        }
        else
        {
            GenerateWorld(seed);
        }
    }

    void Awake()
    {
        GlobalInventory = gameObject.AddComponent<Inventory>();
    }

    void GenerateWorld(int seed)
    {
        Random.InitState(seed);


    }
}
