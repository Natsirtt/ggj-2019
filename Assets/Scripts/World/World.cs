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

    public class Tile
    {
        public enum Type
        {
            Grass,
            Snow,
            Mountain,
            Campfire,
            Hearth,
            Tree
        }

        public Vector2Int Coordinates { get; private set; }
        public Type TileType { get; set; }

        public World.Tile Parent { get; set; }
        public float DistanceToTarget { get; set; }
        public float Cost { get; set; }
        public float F { get { return DistanceToTarget >= 0.0f && Cost >= 0.0f ? DistanceToTarget + Cost : -1.0f; } }

        public bool IsTraversable()
        {
            return 
                TileType != Type.Mountain && 
                TileType != Type.Campfire && 
                TileType != Type.Tree;
        }

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
        Vector2Int previousPatchCenter = hearthGridPos;
        int theoreticalAvailableWood = parameters.resources.startingWoodAmount;
        for (int i = 0; i < numberOfPaths; i++)
        {

        }

        float snowDensity = 0.2f;
        for (int i = -200; i < 200; i++)
        {
            for (int j = -200; j < 200; j++)
            {
                if (new Vector2Int(i, j) == hearthGridPos)
                {
                    continue;
                }

                Tile.Type type = Tile.Type.Grass;
                if (Random.Range(0.0f, 1.0f) <= snowDensity)
                {
                    type = Tile.Type.Snow;
                }
                Tiles.Add(new Vector2Int(i, j), new Tile(new Vector2Int(i, j), type));
            }
        }
    }
}
