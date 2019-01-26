﻿using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private TileBase FirePlaceTile;

    private float currentBurnRatePerSecond;
    private int currentRadiusOfInfluence;
    private float currentWorkerSpawnRate;

    private float burnProgress;
    private float spawnProgress;
    private Inventory globalInventory;
    private World world;

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
        Activate();
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    public void Activate()
    {
        burnProgress = 0f;
        currentRadiusOfInfluence = radiusOfInfluence;
        currentBurnRatePerSecond = burnRatePerSecond;
        currentWorkerSpawnRate = workerSpawnRatePerSecond;
        influence = World.Get().GetTilesInRadius(TilePosition(), CurrentRadiusOfInfluence);
        Debug.Log("Tiles in influence radius: " + influence.Count.ToString());
        foreach(World.Tile tile in influence)
        {
            tile.SetIsInSnow(false);
            if (tile.TileType == World.Tile.Type.Tree)
            {
                Jobs.QueueJob(tile.Coordinates, JobDispatcher.Job.Type.Chop);
                Debug.DrawLine(transform.position, World.Get().GetWorldLocation(tile.Coordinates));
            }
        }
        Debug.Log("Found jobs: " + Jobs.Count.ToString());
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
        World.Get().TilemapFires.SetTile(new Vector3Int(GridTile.Coordinates.x, GridTile.Coordinates.y, 0), FirePlaceTile);
    }
}
