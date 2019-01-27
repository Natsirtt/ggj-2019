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

public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public BitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BitMaskAttribute))]
public class EnumBitMaskPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        var typeAttr = attribute as BitMaskAttribute;
        // Add the actual int value behind the field name
        label.text = label.text + "(" + prop.intValue + ")";
        prop.intValue = EditorExtension.DrawBitMaskField(position, prop.intValue, typeAttr.propType, label);
    }
}

public static class EditorExtension
{
    public static int DrawBitMaskField(Rect aPosition, int aMask, System.Type aType, GUIContent aLabel)
    {
        var itemNames = System.Enum.GetNames(aType);
        var itemValues = System.Enum.GetValues(aType) as int[];

        int val = aMask;
        int maskVal = 0;
        for (int i = 0; i < itemValues.Length; i++)
        {
            if (itemValues[i] != 0)
            {
                if ((val & itemValues[i]) == itemValues[i])
                    maskVal |= 1 << i;
            }
            else if (val == 0)
                maskVal |= 1 << i;
        }
        int newMaskVal = EditorGUI.MaskField(aPosition, aLabel, maskVal, itemNames);
        int changes = maskVal ^ newMaskVal;

        for (int i = 0; i < itemValues.Length; i++)
        {
            if ((changes & (1 << i)) != 0)            // has this list item changed?
            {
                if ((newMaskVal & (1 << i)) != 0)     // has it been set?
                {
                    if (itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
                    {
                        val = 0;
                        break;
                    }
                    else
                        val |= itemValues[i];
                }
                else                                  // it has been reset
                {
                    val &= ~itemValues[i];
                }
            }
        }
        return val;
    }
}

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
public struct TileVariation
{
    public TileBase Normal;
    public TileBase Snowed;
}

[Serializable]
public struct TileContainer
{
    public List<TileVariation> Variations;

    public TileBase GetRandomTile(bool snowed)
    {
        if (Variations.Count <= 0)
            return null;

        TileVariation result = Variations[Random.Range(0, Variations.Count)];

        return result.Snowed == null ? result.Normal : (snowed ? result.Snowed : result.Normal);
    }
}

[Serializable]
public class TileNeighborTransition
{
    [BitMaskAttribute(typeof(World.Tile.NeighborsThatAreDifferent))]
    public World.Tile.NeighborsThatAreDifferent Mask;

    public TileVariation TileVariation;
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
            House,
            MAX
        }

        public enum NeighborsThatAreDifferent
        {
            North       = 0x01,
            East        = North << 1,
            South       = East << 1,
            West        = South << 1,
        }

        public Vector2Int Coordinates { get; private set; }
        public Type TileType { get; set; }
        public bool IsInSnow { get; private set; }

        public void SetIsInSnow(bool flag)
        {
            IsInSnow = flag;
            
            ChangedIsInSnow();
            List<World.Tile> adjacentTiles = World.Get().GetTilesInSquare(Coordinates, 1);
            //List<World.Tile> adjacentTiles = GetAdjacentTiles(World.Get().Tiles, this);
           // List<World.Tile> adjacentTiles = new List<World.Tile>();
           // adjacentTiles.Add(World.Get().Tiles[Coordinates + Vector2Int.left]);
            foreach (World.Tile t in adjacentTiles)
                t.ChangedIsInSnow();
        }

        public void ChangedIsInSnow()
        {
            int neighborSameMask = 0;
            TileBase newVisual = World.Get().TileTypes[(int)Type.Grass].GetRandomTile(IsInSnow);

            List<World.Tile> adjacentTiles = World.Get().GetTilesInSquare(Coordinates, 1);
            //List<World.Tile> adjacentTiles = GetAdjacentTiles(World.Get().Tiles, this);
            // List<World.Tile> adjacentTiles = new List<World.Tile>();
            // adjacentTiles.Add(World.Get().Tiles[Coordinates + Vector2Int.up]);
            //adjacentTiles.Add(World.Get().Tiles[Coordinates + Vector2Int.down]);

            foreach (World.Tile t in adjacentTiles)
            {
                if (t.IsInSnow == IsInSnow)
                {
                    continue;
                }

                Vector2Int offset = t.Coordinates - Coordinates;
                if (offset.x < 0 && offset.y == 0)
                    neighborSameMask |= (int)NeighborsThatAreDifferent.West;
                else if (offset.x > 0 && offset.y == 0)
                    neighborSameMask |= (int)NeighborsThatAreDifferent.East;
                else if (offset.y < 0 && offset.x == 0)
                    neighborSameMask |= (int)NeighborsThatAreDifferent.South;
                else if (offset.y > 0 && offset.x == 0)
                    neighborSameMask |= (int)NeighborsThatAreDifferent.North;
            }

            TileNeighborTransition variation = Array.Find(World.Get().NeighborTransitions, x => (int)x.Mask == neighborSameMask);
            if(variation != null)
            {
                newVisual = IsInSnow ? variation.TileVariation.Snowed : variation.TileVariation.Normal;
            }
            World.Get().Tilemaps[(int)Type.Grass].SetTile(new Vector3Int(Coordinates.x, Coordinates.y, 0), newVisual);
        }
        
        public static List<World.Tile> GetAdjacentTiles(Dictionary<Vector2Int, World.Tile> inGrid, World.Tile tile)
        {
            List<World.Tile> temp = new List<World.Tile>();

            Vector2Int coordinates = tile.Coordinates;

            if (inGrid.ContainsKey(coordinates + Vector2Int.up))
            {
                temp.Add(inGrid[coordinates + Vector2Int.up]);
            }

            if (inGrid.ContainsKey(coordinates + Vector2Int.down))
            {
                temp.Add(inGrid[coordinates + Vector2Int.down]);
            }

            if (inGrid.ContainsKey(coordinates + Vector2Int.left))
            {
                temp.Add(inGrid[coordinates + Vector2Int.left]);
            }

            if (inGrid.ContainsKey(coordinates + Vector2Int.right))
            {
                temp.Add(inGrid[coordinates + Vector2Int.right]);
            }

            return temp;
        }

        public Tile Parent { get; set; }
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
                TileType != Type.Mountain && TileType != Type.Hearth && TileType != Type.Campfire && TileType != Type.House;
        }

        public Tile(Vector2Int coordinates, Type type)
        {
            Coordinates = coordinates;
            TileType = type;
            IsInSnow = true;
        }
    }

    public void GameOverButYouWin()
    {
        FindObjectOfType<Controller>().BlockGameplayInputs = true;
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
    public GameObject Hearth { get; private set; }

    [NamedArray(typeof(Tile.Type))]
    public TileContainer[] TileTypes = new TileContainer[(int)Tile.Type.MAX];
    
    [SerializeField]
    public TileNeighborTransition[] NeighborTransitions;

    [NamedArray(typeof(Tile.Type))]
    public Tilemap[] Tilemaps = new Tilemap[(int)Tile.Type.MAX];

    [SerializeField]
    private WorldGenerationParameters generationParameters = null;
    public WorldGenerationParameters GenerationParameters { get { return generationParameters; } }

    [SerializeField]
    [Tooltip("Leave the seed to 0 for using the current time, or provide your seed of choice.")]
    private int seed = 0;

    public GameObject workerPrefab;
    public GameObject firePrefab;
    public GameObject hearthPrefab;

    public List<GameObject> Workers { get; private set; }
    public List<GameObject> Fires { get; private set; }

    public void SpawnWorker(Fire fire)
    {
        // This order by with weighed random will shuffle the list but segregate the shuffle grass tiled as more important than the others
        Vector2Int pos = fire.GetInfluence().OrderBy(t => Random.value * (t.TileType == Tile.Type.Grass ? 1f : 10f)).ToList().Find(t => t.TileType == Tile.Type.Grass || t.TileType == Tile.Type.Tree).Coordinates;
        GameObject worker = Instantiate<GameObject>(workerPrefab, GetWorldLocation(pos), Quaternion.identity);
        AgentJobHandler jobsScript = worker.GetComponent<AgentJobHandler>();
        if (jobsScript != null) {
            Workers.Add(worker);
            jobsScript.Fire = fire;
        }
    }

    public GameObject GetClosestFire(Vector2 worldLocation) {
        int nearestDistance = 99999;
        GameObject closest = null;
        Vector2Int location = GetGridLocation(worldLocation);
        foreach(GameObject fire in Fires)
        {
            int distance = GetManhattanDistance(location, fire.GetComponent<Fire>().TilePosition());
            if (distance < nearestDistance)
            {
                distance = nearestDistance;
                closest = fire;
            }
        }
        return closest;
    }

    public void ChoppedTree(Vector2Int tilePosition)
    {
        SetTileType(tilePosition, Tile.Type.Grass);
        GlobalInventory.AddWood(GenerationParameters.resources.woodPerTree);
    }

    public GameObject GetClosestIdleWorker (Vector2 location)
    {
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
                Debug.LogWarning("Cannot build path!");
            }
        }
        return nearestWorker;
    }

    public void SpawnHearth(Vector2 worldLocation)
    {
        Vector2Int gridPos = GetGridLocation(worldLocation);
        SetTileType(gridPos, Tile.Type.Hearth);
        GameObject hearth = Instantiate<GameObject>(hearthPrefab, worldLocation, Quaternion.identity);
        //float depth = hearthParticleSystemPrefab.transform.position.z;
        //GameObject hearthParticleSystem = Instantiate<GameObject>(hearthParticleSystemPrefab, (Vector3)GetWorldLocation(gridPos) + new Vector3(0, 0, depth), hearthParticleSystemPrefab.transform.rotation);
        Fire fireScript = hearth.GetComponent<Fire>();
        fireScript.SetWorldTile(Tiles[gridPos]);
        Fires.Add(hearth);
        Camera.main.transform.position = new Vector3(worldLocation.x, worldLocation.y, Camera.main.transform.position.z);
        Debug.Log("Created Hearth at grid position " + gridPos);
        Hearth = hearth;
    }

    public void SpawnCampFire(Vector2 worldLocation)
    {
        var gridPos = GetGridLocation(worldLocation);
        SetTileType(gridPos, Tile.Type.Campfire);
        GameObject fire = Instantiate<GameObject>(firePrefab, worldLocation, Quaternion.identity);
        //float depth = fireParticleSystemPrefab.transform.position.z;
        //GameObject fireParticleSystem = Instantiate<GameObject>(fireParticleSystemPrefab, (Vector3)GetWorldLocation(gridPos) + new Vector3(0, 0, depth), fireParticleSystemPrefab.transform.rotation);
        Fire fireScript = fire.GetComponent<Fire>();
        if (fireScript != null)
        {
            Vector2Int tileLocationInGridSpace = GetGridLocation(fire.transform.position);
            Tile tileToGiveToFireScript = Tiles[tileLocationInGridSpace];
            fireScript.SetWorldTile(tileToGiveToFireScript);
            Fires.Add(fire);
        }
        else
        {
            Debug.LogError("Failed to spawn a campfire. The coming days are going to be cold...");
        }
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

    public static int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool IsInWorld(Vector2Int gridLocation)
    {
        Vector2Int halfGridSize = GetHalfGridSize();
        return -halfGridSize.x <= gridLocation.x && gridLocation.x <= halfGridSize.x
            && -halfGridSize.y <= gridLocation.y && gridLocation.y <= halfGridSize.y;
    }

    public void GameOver(Tile hearthTile)
    {
        TileBase tileToRender = TileTypes[(int)Tile.Type.Hearth].Variations[1].Normal;
        Tilemaps[(int)Tile.Type.Hearth].SetTile(new Vector3Int(hearthTile.Coordinates.x, hearthTile.Coordinates.y, 0), tileToRender);
    }

    public void SetFireRenderTile (Tile tile, bool extinguish=true)
    {
        int index = 0;
        if (extinguish)
        {
            index = 1;
        }
        TileBase tileToRender = TileTypes[(int)Tile.Type.Campfire].Variations[index].Normal;
        Tilemaps[(int)Tile.Type.Campfire].SetTile(new Vector3Int(tile.Coordinates.x, tile.Coordinates.y, 0), tileToRender);
    }

    public void SetTileType(Vector2Int pos, Tile.Type type)
    {
        if (!Tiles.ContainsKey(pos))
        {
            Tiles.Add(pos, new Tile(pos, Tile.Type.Grass));
        }

        Tiles[pos].TileType = type;

        TileBase tileToRender = type == Tile.Type.Hearth ? TileTypes[(int)Tile.Type.Hearth].Variations[0].Normal : TileTypes[(int)type].GetRandomTile(Tiles[pos].IsInSnow);
        if (tileToRender == null)
        {
            Debug.LogError("Could not find a valid tile to render for type " + type);
            return;
        }

        Tilemaps[(int)type].SetTile(new Vector3Int(pos.x, pos.y, 0), tileToRender);
        if (type == Tile.Type.Grass)
        {
            // Clear props rendering
            for (int i = 0; i < (int)Tile.Type.MAX; i++)
            {
                if ((Tile.Type) i != Tile.Type.Grass && Tilemaps[i] != null)
                {
                    Tilemaps[i].SetTile(new Vector3Int(pos.x, pos.y, 0), null);
                }
            }
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

    public static List<Tile> SortByDistance(List<Tile> tiles, Vector2Int gridLocation)
    {
        return tiles.OrderBy(o => GetManhattanDistance(gridLocation, o.Coordinates)).ToList();
    }

    public List<Tile> GetTilesInRadius(Vector2Int gridLocation, int radius)
    {
        List<Tile> temp = new List<World.Tile>();
        float radiusSquared = radius * radius;
        int xMin = gridLocation.x - radius;
        int xMax = gridLocation.x + radius;
        int yMin = gridLocation.y - radius;
        int yMax = gridLocation.y + radius;
        Vector2Int coordinates = new Vector2Int(xMin, yMin);
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                coordinates.x = x;
                coordinates.y = y;
                if (GetManhattanDistance(gridLocation, coordinates) < radius && Tiles.ContainsKey(coordinates))
                {
                    temp.Add(Tiles[coordinates]);
                }
            }
        }
        return temp;
    }

    public List<Tile> GetTilesInSquare(Vector2Int gridLocation, int radius)
    {
        List<Tile> temp = new List<World.Tile>();
        float radiusSquared = radius * radius;
        int xMin = gridLocation.x - radius;
        int xMax = gridLocation.x + radius;
        int yMin = gridLocation.y - radius;
        int yMax = gridLocation.y + radius;
        Vector2Int coordinates = new Vector2Int(xMin, yMin);
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                coordinates.x = x;
                coordinates.y = y;
                if (Tiles.ContainsKey(coordinates))
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
    {}

    void Awake()
    {
        GlobalInventory = gameObject.AddComponent<Inventory>();
        Fires = new List<GameObject>();
        Workers = new List<GameObject>();
    }

    private List<Direction> GetRandomDirectionsList(int minListSize, int maxListSize)
    {
        var directions = new List<Direction>();
        for (int i = 0; i < Random.Range(minListSize, maxListSize); i++)
        {
            directions.Add(GetRandomDirection());
        }
        return directions;
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
        SpawnHearth(GetWorldLocation(hearthGridPos));

        GlobalInventory.CurrentWood = parameters.resources.startingWoodAmount;

        // Seeding woods paths
        int numberOfPaths = Random.Range(parameters.forests.numberOfPathsRange.x, parameters.forests.numberOfPathsRange.y);
        Debug.Log("Generating " + numberOfPaths + " forest paths...");
        Vector2Int previousPatchCenter = hearthGridPos;
        int theoreticalAvailableWood = parameters.resources.startingWoodAmount;
        // Generating all paths
        for (int pathID = 0; pathID < numberOfPaths; pathID++)
        {
            theoreticalAvailableWood = GenerateForestPath(hearthGridPos, theoreticalAvailableWood, GetRandomDirectionsList(10, 50), parameters);
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
        int numberOfPatchesConsistentWithDirection = 0;
        // Generating all patches of the current path
        for (int patchID = 0; patchID < numberOfPatches; patchID++)
        {
            if (numberOfPatchesConsistentWithDirection == parameters.forests.numberOfPatchesWithConsistentDirection)
            {
                directions = GetRandomDirectionsList(10, 30);
                numberOfPatchesConsistentWithDirection = 0;
            }
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
                if (Random.value <= patchDensity && t.TileType != Tile.Type.Hearth)
                {
                    SetTileType(t.Coordinates, Tile.Type.Tree);
                    theoreticalWoodAmount += treeWoodAmount;
                }
            }
            numberOfPatchesConsistentWithDirection++;
        }

        return theoreticalWoodAmount;
    }
}
