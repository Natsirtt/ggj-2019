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

    public void TakeJob(JobDispatcher.Job job)
    {
        // trigger animation
        gameObject.GetComponent<Pathfollowing>().MoveToLocation(World.Get().GetWorldLocation(job.Coordinates));
        Job = job;
        IsIdle = false;
    }

    public void AbortJob()
    {
        // TODO 
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        IsIdle = true;
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
        if (IsIdle)
        {
            JobDispatcher availableJobs = Fire.Jobs;
            if (availableJobs.HasJobs())
            {
                JobDispatcher.Job job = availableJobs.GetNearestJob(TilePosition());
                TakeJob(job);
            }
            else
            {

            }
        }
    }
}
