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
            if (job.JobType != JobDispatcher.Job.Type.Expedition)
            {
                Fire.Jobs.QueueJob(job);
            }
            return;
        }
        Job = job;
        job.NumWorkers += 1;
        if (job.IsReady() && job.JobType == JobDispatcher.Job.Type.Expedition)
        {
            Fire.Jobs.dequeueExpedition();
        }
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
                if (Job.IsReady())
                {
                    JobProgress += Time.deltaTime;
                }
                gameObject.GetComponent<Animator>().SetBool("isChopping", true);
                if (JobProgress >= Job.Duration())
                {
                    gameObject.GetComponent<Animator>().SetBool("isChopping", false);
                    Debug.Log("Changing Tile of Type " +  World.Get().Tiles[Job.Coordinates].TileType.ToString());
                    if (Job.JobType == JobDispatcher.Job.Type.Chop)
                    {
                        World.Get().ChoppedTree(Job.Coordinates);
                    }
                    else
                    {
                        World.Get().SpawnCampFire(World.Get().GetWorldLocation(Job.Coordinates));
                    }
                    JobProgress = 0f;
                    Job = null;
                    IsIdle = true;
                }
            }
        }
    }
}
