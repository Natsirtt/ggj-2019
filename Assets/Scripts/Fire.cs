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
    private float burnRatePerSecondToWin = 1f;

    [SerializeField]
    private int radiusOfInfluence = 0;

    [SerializeField]
    private int radiusOfInfluenceIncrease = 5;

    [SerializeField]
    private float workerSpawnRatePerSecond = 0f;

    [SerializeField]
    private float workerSpawnRateIncrease = 0f;

    private float currentBurnRatePerSecond;
    private float currentWorkerSpawnRate;
    public int CurrentRadiusOfInfluence { get; set; }
    public float CurrentBurnRate { get {return currentBurnRatePerSecond; } }

    public int DefaultRadius { get { return radiusOfInfluence; } }

    private float burnProgress;
    private float spawnProgress;
    private Inventory globalInventory;
    private World world;
    private float nextHouseSpawnTick;
    private bool needsToActivate = false;

    private List<GameObject> listOfAssociatedWorkers;

    public JobDispatcher Jobs {get; private set;}

    private List<World.Tile> influence;

    public List<World.Tile> GetInfluence() { return influence; }

    void Awake()
    {
        Jobs = gameObject.AddComponent<JobDispatcher>();
        listOfAssociatedWorkers = new List<GameObject>();
    }

    public World.Tile GridTile { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        world = World.Get();
        globalInventory = world.GlobalInventory;
        nextHouseSpawnTick = Time.time + Random.Range(world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.x, world.GenerationParameters.infrastructures.houseSpawnPerSecondInterval.y);
        needsToActivate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (needsToActivate)
        {
            Activate();
            needsToActivate = false;
        }
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
                burnProgress = 0f;
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
                GameObject worker = world.SpawnWorker(this);
                listOfAssociatedWorkers.Add(worker);
                spawning -= 1;

            }
        }
        foreach (JobDispatcher.Job job in Jobs.chopJobs())
        {
            Debug.DrawLine(transform.position, world.GetWorldLocation(job.Coordinates));
        }
    }

    public void WorkerLeaving(GameObject worker)
    {
        listOfAssociatedWorkers.Remove(worker);
    }

    public int NumAssociatedWorkers()
    {
        return listOfAssociatedWorkers.Count();
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
        if (GridTile.TileType == World.Tile.Type.Campfire)
        {
            world.SetFireRenderTile(GridTile, 1);
        }
        var emission = gameObject.GetComponent<ParticleSystem>().emission;
        emission.enabled = false;
        ComputeInfluence();
    }

    void ComputeInfluence()
    {
        foreach (World.Tile tile in influence)
        {
            tile.SetIsInSnow(true);
        }
        influence = World.SortByDistance(World.Get().GetTilesInRadius(TilePosition(), CurrentRadiusOfInfluence), TilePosition());
        foreach (World.Tile tile in influence)
        {
            tile.SetIsInSnow(false);
            if (tile.TileType == World.Tile.Type.Tree)
            {
                Jobs.QueueJob(tile.Coordinates, JobDispatcher.Job.Type.Chop);
            }
        }
    }

    public void Activate()
    {
        burnProgress = 0f;
        CurrentRadiusOfInfluence = radiusOfInfluence;
        currentBurnRatePerSecond = burnRatePerSecond;
        currentWorkerSpawnRate = workerSpawnRatePerSecond;
        ComputeInfluence();
        var ps = GetComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.enabled = true;
        if (GridTile.TileType == World.Tile.Type.Hearth)
        {
            // Win progress between 0 and 1
            float winProgress = currentBurnRatePerSecond / burnRatePerSecondToWin;
            Debug.Log("Current burn rate: " + currentBurnRatePerSecond + " - Win progress: " + winProgress);
            var main = ps.main;
            var shape = ps.shape;
            main.startSize = 5f * winProgress;
            emission.rateOverTime = 100 * winProgress;
            shape.radius = 1.5f * winProgress;
        }
        else if (GridTile.TileType == World.Tile.Type.Campfire)
        {
            world.SetFireRenderTile(GridTile, 0);
        }
    }

    public void Feed()
    {
        CurrentRadiusOfInfluence += radiusOfInfluenceIncrease;
        currentBurnRatePerSecond += burnRatePerSecondIncrease;
        currentWorkerSpawnRate += workerSpawnRateIncrease;
        ComputeInfluence();
        // This is only called for the Hearth.
        ParticleSystem ps = GetComponent<ParticleSystem>();
        // Win progress between 0 and 1
        float winProgress = currentBurnRatePerSecond / burnRatePerSecondToWin;
        Debug.Log("Current burn rate: " + currentBurnRatePerSecond + " - Win progress: " + winProgress);
        var emission = ps.emission;
        var main = ps.main;
        var shape = ps.shape;
        main.startSize = 5f * winProgress;
        emission.rateOverTime = 100 * winProgress;
        shape.radius = 1.5f * winProgress;
        if (winProgress >= 1.0f)
        {
            world.GameOverButYouWin();
        }
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
        CurrentRadiusOfInfluence -= radiusOfInfluenceIncrease;
        currentBurnRatePerSecond -= burnRatePerSecondIncrease;
        if (currentBurnRatePerSecond <= 0)
        {
            Deactivate();
        }
        else
        {
            ComputeInfluence();
        }
    }

    public void SetWorldTile(World.Tile tileToGiveToFireScript)
    {
        GridTile = tileToGiveToFireScript;
    }
}
