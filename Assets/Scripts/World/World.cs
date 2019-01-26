﻿using UnityEngine;
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

    [SerializeField]
    private Vector2 TileSize = new Vector2(32, 32);

    public Dictionary<Vector2Int, Tile> Tiles { get; private set; }
    public Inventory GlobalInventory { get; private set; }
    public Vector2Int GridSize { get; private set; }

    [SerializeField]
    private WorldGenerationParameters generationParameters = null;

    // Leave the seed to 0 for using the current time. Provide a hardcoded seed otherwise.
    [SerializeField]
    private int seed = 0;

    public static Vector2Int GetGridLocation(Vector2 worldLocation)
    {
        Vector2 transformedLocation = worldLocation / World.Get().TileSize;
        return new Vector2Int((int)transformedLocation.x, (int)transformedLocation.y);
    }

    public static Vector2 GetWorldLocation(Vector2Int gridLocation)
    {
        Vector2Int transformedLocation = gridLocation * new Vector2Int((int)World.Get().TileSize.x, (int)World.Get().TileSize.y);
        return new Vector2((float)transformedLocation.x, (float)transformedLocation.y);
    }

    void Start()
    {
        if (generationParameters == null)
        {
            Debug.LogError("No World Generation Parameters asset provided! World is self destructing.");
            Destroy(gameObject);
            return;
        }

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
        Tiles = new Dictionary<Vector2Int, Tile>();
        Random.InitState(seed);
        GridSize = parameters.grid.Size;

        // Creating hearth
        Vector2Int maxHearthGridPosition = GridSize- parameters.infrastructures.hearthMinDistanceFromMapEdge;
        var hearthGridPos = new Vector2Int(Random.Range(-maxHearthGridPosition.x, maxHearthGridPosition.x), Random.Range(-maxHearthGridPosition.y, maxHearthGridPosition.y));
        Tiles.Add(hearthGridPos, new Tile(hearthGridPos, Tile.Type.Hearth));
        Debug.Log("Created Hearth at grid position " + hearthGridPos);

        // Seeding woods paths
        int numberOfPaths = Random.Range(parameters.forests.numberOfPathsRange.x, parameters.forests.numberOfPathsRange.y);
        Debug.Log("Generating " + numberOfPaths + " forest paths...");
        for (int i = 0; i < numberOfPaths; i++)
        {

        }
    }
}
