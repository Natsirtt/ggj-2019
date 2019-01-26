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

    public class Tile
    {
        public enum Type
        {
            Grass,
            Mountain,
            Campfire,
            Hearth,
            Tree
        }

        public Vector2Int Coordinates { get; private set; }
        public Type TileType { get; set; }
        public bool IsInSnow { get; set; }

        public World.Tile Parent { get; set; }
        public float DistanceToTarget { get; set; }
        public float Cost { get; set; }
        public float F { get { return DistanceToTarget >= 0.0f && Cost >= 0.0f ? DistanceToTarget + Cost : -1.0f; } }

        public bool IsTraversable()
        {
            return 
                TileType != Type.Mountain
                && TileType != Type.Campfire
                && TileType != Type.Hearth
                && TileType != Type.Tree;
        }

        public Tile(Vector2Int coordinates, Type type)
        {
            Coordinates = coordinates;
            TileType = type;
            IsInSnow = true;
        }
    }

    public enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public Vector2Int GetDirectionVector(Direction d)
    {
        switch (d)
        {
            case Direction.North:
                return new Vector2Int(0, 1);
            case Direction.NorthEast:
                return new Vector2Int(1, 1);
            case Direction.East:
                return new Vector2Int(1, 0);
            case Direction.SouthEast:
                return new Vector2Int(1, -1);
            case Direction.South:
                return new Vector2Int(0, -1);
            case Direction.SouthWest:
                return new Vector2Int(-1, -1);
            case Direction.West:
                return new Vector2Int(-1, 0);
            case Direction.NorthWest:
                return new Vector2Int(-1, 1);
        }
        throw new Exception("No");
    }

    public List<Direction> GetDirectionOpposites(Direction d)
    {
        switch (d)
        {
            case Direction.North:
                return new List<Direction>{ Direction.South, Direction.SouthEast, Direction.SouthWest };
            case Direction.NorthEast:
                return new List<Direction>{ Direction.West, Direction.SouthWest, Direction.South };
            case Direction.East:
                return new List<Direction>{ Direction.NorthWest, Direction.West, Direction.SouthWest };
            case Direction.SouthEast:
                return new List<Direction>{ Direction.West, Direction.NorthWest, Direction.North };
            case Direction.South:
                return new List<Direction>{ Direction.North, Direction.NorthWest, Direction.NorthEast };
            case Direction.SouthWest:
                return new List<Direction>{ Direction.North, Direction.NorthEast, Direction.East };
            case Direction.West:
                return new List<Direction>{ Direction.NorthEast, Direction.East, Direction.SouthEast };
            case Direction.NorthWest:
                return new List<Direction>{ Direction.South, Direction.SouthEast, Direction.East };
        }
        throw new Exception("No");
    }

    [SerializeField]
    private Vector2 TileSize = new Vector2(32, 32);

    public Dictionary<Vector2Int, Tile> Tiles { get; private set; }
    public Inventory GlobalInventory { get; private set; }
    public Vector2Int GridSize { get; private set; }

    [SerializeField]
    private WorldGenerationParameters generationParameters = null;

    [SerializeField]
    [Tooltip("Leave the seed to 0 for using the current time, or provide your seed of choice.")]
    private int seed = 0;

    public GameObject workerPrefab;
    public GameObject firePrefab;
    public GameObject hearthPrefab;

    public List<GameObject> Workers { get; private set; }
    public List<GameObject> Fires { get; private set; }

    public void SpawnWorker(Vector2 worldLocation)
    {
        GameObject worker = Instantiate<GameObject>(workerPrefab, worldLocation, Quaternion.identity);
        Workers.Add(worker);
    }

    public void SpawnHearth(Vector2 worldLocation)
    {
        GameObject hearth = Instantiate<GameObject>(hearthPrefab, worldLocation, Quaternion.identity);
        Fires.Add(hearth);
    }

    public void SpawnCampFire(Vector2 worldLocation)
    {
        GameObject fire = Instantiate<GameObject>(firePrefab, worldLocation, Quaternion.identity);
        Fires.Add(fire);
    }

    public Vector2Int GetGridLocation(Vector2 worldLocation)
    {
        Vector2 transformedLocation = worldLocation / TileSize;
        return new Vector2Int((int)transformedLocation.x, (int)transformedLocation.y);
    }

    public Vector2 GetWorldLocation(Vector2Int gridLocation)
    {
        Vector2Int transformedLocation = gridLocation * new Vector2Int((int)TileSize.x, (int)TileSize.y);
        return new Vector2((float)transformedLocation.x, (float)transformedLocation.y);
    }

    public Vector2Int GetHalfGridSize()
    {
        return new Vector2Int(GridSize.x / 2, GridSize.y / 2);
    }

    public Vector2Int? GetNeighbourAt(Vector2Int v, Direction direction)
    {
        Vector2Int result = v + GetDirectionVector(direction);
        if (!IsInWorld(result))
        {
            return null;
        }
        return result;
    }

    public int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool IsInWorld(Vector2Int gridLocation)
    {
        Vector2Int halfGridSize = GetHalfGridSize();
        return -halfGridSize.x <= gridLocation.x && gridLocation.x <= halfGridSize.x
            && -halfGridSize.y <= gridLocation.y && gridLocation.y <= halfGridSize.y;
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

    void Update()
    {
        Debug.DrawLine(Vector2.zero, new Vector2(0, -1), Color.green);
        Debug.DrawLine(Vector2.zero, new Vector2(0, 1), Color.blue);
        Debug.DrawLine(Vector2.zero, new Vector2(-1, 0), Color.cyan);
        Debug.DrawLine(Vector2.zero, new Vector2(1, 0), Color.red);
    }

    void Awake()
    {
        GlobalInventory = gameObject.AddComponent<Inventory>();
        Fires = new List<GameObject>();
        Workers = new List<GameObject>();
    }

    void GenerateWorld(int seed, WorldGenerationParameters parameters)
    {
        Tiles = new Dictionary<Vector2Int, Tile>();
        Random.InitState(seed);
        GridSize = parameters.grid.Size;
        if (GridSize.x % 2 != 0 || GridSize.y % 2 != 0)
        {
            Debug.LogWarning("Grid size " + GridSize + " has an odd component. Even components might work better.");
        }

        for (int x = -GridSize.x / 2; x <= GridSize.x / 2; x++)
        {
            for (int y = -GridSize.y / 2; y <= GridSize.y / 2; y++)
            {
                var pos = new Vector2Int(x, y);
                Tiles.Add(pos, new Tile(pos, Tile.Type.Grass));
            }
        }

        // Creating hearth
        Vector2Int maxHearthGridPosition = GridSize- parameters.infrastructures.hearthMinDistanceFromMapEdge;
        var hearthGridPos = new Vector2Int(Random.Range(-maxHearthGridPosition.x, maxHearthGridPosition.x), Random.Range(-maxHearthGridPosition.y, maxHearthGridPosition.y));
        Tiles[hearthGridPos].TileType = Tile.Type.Hearth;
        SpawnHearth(GetWorldLocation(hearthGridPos));
        Debug.Log("Created Hearth at grid position " + hearthGridPos);

        // Seeding woods paths
        int numberOfPaths = Random.Range(parameters.forests.numberOfPathsRange.x, parameters.forests.numberOfPathsRange.y);
        Debug.Log("Generating " + numberOfPaths + " forest paths...");
        Vector2Int previousPatchCenter = hearthGridPos;
        int theoreticalAvailableWood = parameters.resources.startingWoodAmount;
        // Generating all paths
        for (int pathID = 0; pathID < numberOfPaths; pathID++)
        {
            // TODO not hardcode this? have heuristics for it?
            var directions = new List<Direction>();
            for (int i = 0; i < Random.Range(4, 20); i++)
            {
                directions.Add((Direction) Random.Range(0, 7));
            }
            theoreticalAvailableWood = GenerateForestPath(hearthGridPos, theoreticalAvailableWood, directions, parameters);
        }
    }

    int GenerateForestPath(Vector2Int hearthPosition, int theoreticalWoodAmount, List<Direction> directions, WorldGenerationParameters parameters)
    {
        theoreticalWoodAmount -= parameters.forests.patchesDifficultyDistanceModifier;
        if (theoreticalWoodAmount <= parameters.resources.expeditionWoodCostPerTile)
        {
            Debug.LogError("Generating new forest path with an amount of wood less than or equal to the amount it costs to do a 1-tile expedition is not possible. Wood amount was " + theoreticalWoodAmount + ", minimum is " + (parameters.resources.expeditionWoodCostPerTile + 1));
            return 0;
        }
        int numberOfPatches = Random.Range(parameters.forests.patchesPerPathRange.x, parameters.forests.patchesPerPathRange.y);
        Debug.Log("\t- Generating " + numberOfPatches + " patches");
        Vector2Int seedPosition = hearthPosition;
        // Generating all patches of the current path
        for (int patchID = 0; patchID < numberOfPatches; patchID++)
        {
            // Snaking away using the resources
            while (theoreticalWoodAmount > 0 || theoreticalWoodAmount < parameters.resources.expeditionWoodCostPerTile)
            {
                Direction direction = directions[Random.Range(0, directions.Count - 1)];
                // Remove the direction's "opposite" so that a forest path always goes towards a similar direction-ish
                foreach (Direction dir in GetDirectionOpposites(direction))
                {
                    directions.Remove(dir);
                }
                if (directions.Count == 0)
                {
                    Debug.LogError("List of directions was empty!");
                    direction = (Direction) Random.Range(0, 7);
                }
                seedPosition += GetDirectionVector(direction);
                theoreticalWoodAmount -= parameters.resources.expeditionWoodCostPerTile;
            }

            // Creating the patch
            float patchDensity = Random.Range(parameters.forests.patchDensityRange.x, parameters.forests.patchDensityRange.y);
            int treeWoodAmount = parameters.resources.woodPerTree;
            int patchWoodMaxAmount = Random.Range(parameters.forests.woodAmountRangePerPatch.x, parameters.forests.woodAmountRangePerPatch.y);
            Debug.Log("\t\t+ Generating patch " + patchID + ". Core is at " + seedPosition);
            int patchHalfSize = Random.Range(parameters.forests.minPatchEuclidianRadius, parameters.forests.maxPatchEuclidianRadius);
            for (int x = -patchHalfSize; x <= patchHalfSize; x++)
            {
                for (int y = -patchHalfSize; y <= patchHalfSize; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (theoreticalWoodAmount >= patchWoodMaxAmount)
                    {
                        break;
                    }
                    if (!IsInWorld(pos))
                    {
                        continue;
                    }
                    if (Random.value <= patchDensity)
                    {
                        Tiles[pos].TileType = Tile.Type.Tree;
                        theoreticalWoodAmount += treeWoodAmount;
                    }
                }
            }
        }

        return theoreticalWoodAmount;
    }
}
