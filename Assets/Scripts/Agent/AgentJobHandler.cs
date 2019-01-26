using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentJobHandler : MonoBehaviour
{
    public bool IsIdle {
        get; set;
    }

    public JobDispatcher.Job Job
    {
        get; set;
    }

    public Fire Fire { set; get; }
    Pathfollowing pathFollowing;
    public float JobProgress { set; get; }

    public void TakeJob(JobDispatcher.Job job)
    {
        // trigger animation
        if (!pathFollowing.MoveToLocation(World.Get().GetWorldLocation(job.Coordinates)))
        {
            Fire.Jobs.QueueJob(job);
            return;
        }
        
        Job = job;
        IsIdle = false;
    }

    public void AbortJob()
    {
        // TODO 
    }
    
    void Awake()
    {
        Job = null;
        JobProgress = 0f;
        IsIdle = true;
        pathFollowing = gameObject.GetComponent<Pathfollowing>();
    }

    public Vector2 WorldPosition()
    {
        return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
    }

    public Vector2Int TilePosition()
    {
        return World.Get().GetGridLocation(WorldPosition());
    }
    // Update is called once per frame
    void Update()
    {
        // somehow check if you are at the jobsite and once done remove the job
        if (Job == null)
        {
            JobDispatcher availableJobs = Fire.Jobs;
            if (availableJobs.HasJobs())
            {
                JobDispatcher.Job job = availableJobs.GetNearestJob(TilePosition());
                TakeJob(job);
                IsIdle = false;
            }
            else
            {
                // TODO look for next fire
            }
        }
        else
        {
            if (!pathFollowing.CurrentPath.HasPath())
            {
                JobProgress += Time.deltaTime;
                gameObject.GetComponent<Animator>().SetBool("isChopping", true);
                if (JobProgress >= Job.Duration())
                {
                    gameObject.GetComponent<Animator>().SetBool("isChopping", false);
                    Debug.Log("Changing Tile of Type " +  World.Get().Tiles[Job.Coordinates].TileType.ToString());
                    World.Get().SetTileType(Job.Coordinates, World.Tile.Type.Grass);
                    JobProgress = 0f;
                    Job = null;
                    IsIdle = true;
                }
            }
        }
    }
}
