using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Fire : MonoBehaviour
{
    [SerializeField]
    private float burnRatePerSecond = 0f;

    [SerializeField]
    private float burnRatePerSecondIncrease = 1f;

    [SerializeField]
    private int radiusOfInfluence = 0;

    [SerializeField]
    private int radiusOfInfluenceIncrease = 5;

    [SerializeField]
    private float workerSpawnRatePerSecond = 0f;

    [SerializeField]
    private float workerSpawnRateIncrease = 0f;

    private float currentBurnRatePerSecond;
    private int currentRadiusOfInfluence;
    private float currentWorkerSpawnRate;

    private float burnProgress;
    private float spawnProgress;
    private Inventory globalInventory;
    private World world;
    private float nextHouseSpawnTick;

    public JobDispatcher Jobs {get; private set;}

    private List<World.Tile> influence;
    void Awake()
    {
        Jobs = gameObject.AddComponent<JobDispatcher>();
    }

    public World.Tile GridTile { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        world = World.Get();
        globalInventory = world.GlobalInventory;
        nextHouseSpawnTick = Random.Range(world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.x, world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.y);
        Activate();
    }

    // Update is called once per frame
    void Update()
    {
        if (nextHouseSpawnTick <= Time.time)
        {
            World.Tile tile = influence.OrderBy(t => Random.value).ToList().Find(t => t.TileType == World.Tile.Type.Grass && !world.GetTilesInRadius(t.Coordinates, world.GenerationParameters.infrastructures.minimumManhattanDistanceBetweenHouses).Any(neighbour => neighbour.TileType == World.Tile.Type.House));
            if (tile != null)
            {
                world.SetTileType(tile.Coordinates, World.Tile.Type.House);
            }
            nextHouseSpawnTick = Time.time + Random.Range(world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.x, world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.y);
        }

        burnProgress += Time.deltaTime * currentBurnRatePerSecond;
        if (burnProgress >= 1f)
        {
            int woodBurnt = Mathf.FloorToInt(burnProgress);
            if (globalInventory.CurrentWood < woodBurnt)
            {
                // Warn the player, dim down the fire?
                Shrink();
                return;
            }
            globalInventory.RemoveWood(woodBurnt);
            burnProgress -= (float) woodBurnt;
        }
        spawnProgress += Time.deltaTime * currentWorkerSpawnRate;
        if (spawnProgress >= 1f)
        {
            int spawning = Mathf.FloorToInt(spawnProgress);
            spawnProgress = spawnProgress - spawning;
            while (spawning > 0)
            {
                // TODO randomize this position
                world.SpawnWorker(WorldPosition(), this);
                spawning -= 1;

            }
        }
        foreach (JobDispatcher.Job job in Jobs.chopJobs())
        {
            Debug.DrawLine(transform.position, world.GetWorldLocation(job.Coordinates));
        }
    }

    public int CurrentRadiusOfInfluence
    {
        get { return currentRadiusOfInfluence; }
        set { currentRadiusOfInfluence = value;  }

    }

    public void Deactivate()
    {
        // TODO change the display as well!
        burnProgress = 0f;
        radiusOfInfluence = 0;
        burnRatePerSecond = 0;
        if (GridTile.TileType == World.Tile.Type.Hearth)
        {
            world.GameOver(GridTile);
        }
    }

    public void Activate()
    {
        burnProgress = 0f;
        currentRadiusOfInfluence = radiusOfInfluence;
        currentBurnRatePerSecond = burnRatePerSecond;
        currentWorkerSpawnRate = workerSpawnRatePerSecond;
        influence = World.SortByDistance(World.Get().GetTilesInRadius(TilePosition(), CurrentRadiusOfInfluence), TilePosition());
        foreach(World.Tile tile in influence)
        {
            tile.SetIsInSnow(false);
            if (tile.TileType == World.Tile.Type.Tree)
            {
                Jobs.QueueJob(tile.Coordinates, JobDispatcher.Job.Type.Chop);
            }
        }
    }

    public void Feed()
    {
        currentRadiusOfInfluence += radiusOfInfluenceIncrease;
        currentBurnRatePerSecond += burnRatePerSecondIncrease;
        currentWorkerSpawnRate += workerSpawnRateIncrease;

    }

    public Vector2 WorldPosition()
    {
        return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
    }

    public Vector2Int TilePosition()
    {
        return world.GetGridLocation(WorldPosition());
    }

    public void Shrink()
    {
        currentRadiusOfInfluence -= radiusOfInfluenceIncrease;
        currentBurnRatePerSecond -= burnRatePerSecondIncrease;
        if (currentBurnRatePerSecond <= 0)
        {
            Deactivate();
        }
    }

    public void SetWorldTile(World.Tile tileToGiveToFireScript)
    {
        GridTile = tileToGiveToFireScript;
    }
}
