using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class World : MonoBehaviour
{
    // Utils

    private static World worldInstance = null;
    static World Get()
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

        public Vector2 coordinates { get; private set; }
        public Type type { get; set; }

        public Tile(Vector2 coordinates, Type type)
        {
            this.coordinates = coordinates;
            this.type = type;
        }
    }

    // World members

    public Dictionary<Vector2, Tile> tiles { get; private set; }

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

    void GenerateWorld(int seed)
    {
        Random.InitState(seed);


    }
}
