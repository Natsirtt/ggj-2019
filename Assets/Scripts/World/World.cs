using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NamedArrayAttribute : PropertyAttribute
{
    public Type TargetEnum;
    public NamedArrayAttribute(Type TargetEnum)
    {
        this.TargetEnum = TargetEnum;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        try
        {
            var config = attribute as NamedArrayAttribute;
            var enum_names = System.Enum.GetNames(config.TargetEnum);
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            var enum_label = enum_names.GetValue(pos) as string;
            // Make names nicer to read (but won't exactly match enum definition).
            enum_label = ObjectNames.NicifyVariableName(enum_label.ToLower());
            label = new GUIContent(enum_label);
        }
        catch
        {
            // keep default label
        }
        EditorGUI.PropertyField(position, property, label, property.isExpanded);
    }
}
#endif

[Serializable]
public struct TileContainer
{
    public List<TileBase> TileSelection;

    public TileBase GetRandomTile()
    {
        return TileSelection.Count <= 0 ? null : TileSelection[Random.Range(0, TileSelection.Count)];
    }
}

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
            Tree,
            MAX
        }

        public Vector2Int Coordinates { get; private set; }
        public Type TileType { get; set; }
        public bool IsInSnow { get; set; }

        public World.Tile Parent { get; set; }
        public float DistanceToTarget { get; set; }
        public float Cost { get; set; }
        public float F { get { return DistanceToTarget >= 0.0f && Cost >= 0.0f ? DistanceToTarget + Cost : -1.0f; } }
        public float Distance(Tile other)
        {
            return (Coordinates - other.Coordinates).magnitude;
        }

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
    public JobDispatcher JobDispatcher {get; private set;}

    [NamedArrayAttribute(typeof(Tile.Type))]
    public TileContainer[] TileTypes = new TileContainer[(int)Tile.Type.MAX];

    [SerializeField]
    private WorldGenerationParameters generationParameters = null;

    [SerializeField]
    [Tooltip("Leave the seed to 0 for using the current time, or provide your seed of choice.")]
    private int seed = 0;

    public GameObject workerPrefab;
    public GameObject firePrefab;
    public GameObject hearthPrefab;

    public Tilemap TilemapGround;
    public Tilemap TilemapTrees;
    public Tilemap TilemapFires;

    public List<GameObject> Workers { get; private set; }
    public List<GameObject> Fires { get; private set; }

    public void SpawnWorker(Vector2 worldLocation)
    {
        GameObject worker = Instantiate<GameObject>(workerPrefab, worldLocation, Quaternion.identity);
        Workers.Add(worker);
    }

    public GameObject GetClosestIdleWorker (Vector2 location)
    {
        List<GameObject> idles = new List<GameObject>();
        float shortestPathLength = 999999f;
        Path currentPath = new Path();
        GameObject nearestWorker = null;
        foreach (GameObject worker in Workers)
        {
            if (worker.GetComponent<AgentJobHandler>().IsIdle)
            {
                if (AStar.BuildPath(Tiles, new Vector2(worker.transform.position.x, worker.transform.position.y), location, ref currentPath))
                {
                    if (currentPath.PathLength() < shortestPathLength)
                    {
                        nearestWorker = worker;
                    }
                }
            }
        }
        return nearestWorker;
    }

    public void SpawnHearth(Vector2 worldLocation)
    {
        GameObject hearth = Instantiate<GameObject>(hearthPrefab, worldLocation, Quaternion.identity);
        Fires.Add(hearth);
        // TODO clear the tiles and queue the trees
    }

    public void SpawnCampFire(Vector2 worldLocation)
    {
        GameObject fire = Instantiate<GameObject>(firePrefab, worldLocation, Quaternion.identity);
        Fire fireScript = fire.GetComponent<Fire>();
        if(fireScript != null)
        {
            Vector2Int tileLocationInGridSpace = GetGridLocation(fire.transform.position);
            Tile tileToGiveToFireScript = Tiles[tileLocationInGridSpace];
            fireScript.SetWorldTile(tileToGiveToFireScript);
            Fires.Add(fire);
        }
        // TODO clear the tiles and queue the trees
    }

    public Vector2Int GetGridLocation(Vector2 worldLocation)
    {
        Vector2 transformedLocation = (worldLocation - TileSize * 0.5f) / TileSize;
        return new Vector2Int(Mathf.RoundToInt(transformedLocation.x), Mathf.RoundToInt(transformedLocation.y));
    }

    public Vector2 GetWorldLocation(Vector2Int gridLocation)
    {
        Vector2Int transformedLocation = gridLocation * new Vector2Int(Mathf.RoundToInt(TileSize.x), Mathf.RoundToInt(TileSize.y));
        return new Vector2((float)transformedLocation.x, (float)transformedLocation.y) + TileSize * 0.5f;
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

    public void SetTileType(Vector2Int pos, Tile.Type type)
    {
        if (!Tiles.ContainsKey(pos))
        {
            Tiles.Add(pos, new Tile(pos, Tile.Type.Grass));
        }

        Tiles[pos].TileType = type;
        TileBase tileToRender = TileTypes[(int)type].GetRandomTile();
        if (tileToRender == null)
        {
            Debug.LogError("Could not find a valid tile to render for type " + type);
            return;
        }

        Vector2 tileMapPos = pos;// * TileSize;
        if (type == Tile.Type.Tree)
        {
            TilemapTrees.SetTile(new Vector3Int((int)tileMapPos.x, (int)tileMapPos.y, 0), tileToRender);
        }
        else
        {
            TilemapGround.SetTile(new Vector3Int((int)tileMapPos.x, (int)tileMapPos.y, 0), tileToRender);
        }
    }

    public Direction GetRandomDirection()
    {
        switch(Random.Range(0, 4))
        {
            case 0:
                return Direction.North;
            case 1:
                return Direction.East;
            case 2:
                return Direction.South;
            case 3:
                return Direction.West;
        }
        throw new Exception("No");
    }

    private List<Tile> GetTilesInRadius(Vector2Int gridLocation, int radius)
    {
        List<Tile> temp = new List<World.Tile>();
        float radiusSquared = radius * radius;
        int xMin = gridLocation.x - radius;
        int xMax = gridLocation.x + radius;
        int yMin = gridLocation.y - radius;
        int yMax = gridLocation.y + radius;
        Vector2Int coordinates = new Vector2Int(xMin, yMin);
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                coordinates.x = x;
                coordinates.y = y;
                if ((coordinates - gridLocation).sqrMagnitude < radiusSquared && Tiles.ContainsKey(coordinates))
                {
                    temp.Add(Tiles[coordinates]);
                }
            }
        }
        return temp;
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
                SetTileType(new Vector2Int(x, y), Tile.Type.Grass);
            }
        }

        // Creating hearth
        Vector2Int maxHearthGridPosition = GetHalfGridSize() - parameters.infrastructures.hearthMinDistanceFromMapEdge;
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
            for (int i = 0; i < Random.Range(10, 50); i++)
            {
                directions.Add(GetRandomDirection());
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
        Debug.Log("Generating " + numberOfPatches + " patches");
        Vector2Int seedPosition = hearthPosition;
        // Generating all patches of the current path
        for (int patchID = 0; patchID < numberOfPatches; patchID++)
        {
            // Snaking away using the resources
            int travelledTilesNb = 0;
            while (theoreticalWoodAmount > 0 && theoreticalWoodAmount > parameters.resources.expeditionWoodCostPerTile)
            {
                Direction direction = directions[Random.Range(0, directions.Count - 1)];
                // Remove the direction's "opposite" so that a forest path always goes towards a similar direction-ish
                foreach (Direction dir in GetDirectionOpposites(direction))
                {
                    directions.RemoveAll(x => x == dir);
                }
                if (directions.Count == 0)
                {
                    Debug.LogError("List of directions was empty!");
                    direction = GetRandomDirection();
                }
                Debug.Log("Travelling " + direction);
                if (!GetNeighbourAt(seedPosition, direction).HasValue)
                {
                    Debug.Log("Seeding new patch would have exited the map. Seeding at edge");
                    break;
                }
                seedPosition += GetDirectionVector(direction);
                travelledTilesNb++;
                theoreticalWoodAmount -= parameters.resources.expeditionWoodCostPerTile;
            }
            Debug.Log("Travelled " + travelledTilesNb + " tiles to seed new patch");

            // Creating the patch
            float patchDensity = Random.Range(parameters.forests.patchDensityRange.x, parameters.forests.patchDensityRange.y);
            int treeWoodAmount = parameters.resources.woodPerTree;
            int patchWoodMaxAmount = Random.Range(parameters.forests.woodAmountRangePerPatch.x, parameters.forests.woodAmountRangePerPatch.y);
            Debug.Log("Generating patch " + patchID + ". Core is at " + seedPosition);
            int patchHalfSize = Random.Range(parameters.forests.minPatchEuclidianRadius, parameters.forests.maxPatchEuclidianRadius);
            foreach (Tile t in GetTilesInRadius(seedPosition, patchHalfSize).OrderBy(x => Random.value).ToList())
            {
                if (theoreticalWoodAmount >= patchWoodMaxAmount)
                {
                    break;
                }
                if (Random.value <= patchDensity)
                {
                    SetTileType(t.Coordinates, Tile.Type.Tree);
                    theoreticalWoodAmount += treeWoodAmount;
                }
            }
        }

        return theoreticalWoodAmount;
    }
}
