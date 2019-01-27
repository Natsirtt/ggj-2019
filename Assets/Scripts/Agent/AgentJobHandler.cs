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

    private bool AtJobSite = false;
    public Fire Fire { set; get; }
    Pathfollowing pathFollowing;
    public float JobProgress { set; get; }

    public float SecondsToDeath;
    public float Despawn;
    private float despawnTimer;
    private float deathTimer;
    private bool isDead = false;

    public List<AudioClip> Audio_ChopWood = new List<AudioClip>();

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
        job.NumWorkersAssigned += 1;
        if (job.NumWorkersAssigned == job.RequiredWorkers && job.JobType == JobDispatcher.Job.Type.Expedition)
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
        AtJobSite = false;
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

    public void PlayChopWood()
    {
        var audioSource = GetComponent<AudioSource>();
        if(audioSource && Audio_ChopWood.Count > 0)
        {
            audioSource.clip = Audio_ChopWood[UnityEngine.Random.Range(0, Audio_ChopWood.Count)];
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            despawnTimer += Time.deltaTime;
            if (despawnTimer > Despawn)
            {
                Destroy(gameObject);
            }
            return;
        }
        // somehow check if you are at the jobsite and once done remove the job
        if (Job == null)
        {
            JobDispatcher availableJobs = Fire.Jobs;
            if (availableJobs.HasJobs())
            {
                JobDispatcher.Job job = availableJobs.GetNearestJob(TilePosition());
                TakeJob(job);
                AtJobSite = false;
                IsIdle = false;
            }
            else
            {
                // TODO look for next fire
                pathFollowing.MoveToRandomLocationInSquare();
            }
        }
        else
        {
            IsIdle = false;
            if (!pathFollowing.CurrentPath.HasPath())
            {
                if (Job.IsReady())
                {
                    JobProgress += Time.deltaTime;
                    gameObject.GetComponent<Animator>().SetBool("isChopping", true);
                }
                else if (!AtJobSite)
                {
                    AtJobSite = true;
                    Job.Arrived();
                }
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
        if (Fire.CurrentRadiusOfInfluence == 0)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer > SecondsToDeath)
            {
                gameObject.GetComponent<Animator>().SetBool("isDead", true);
                isDead = true;
            }
        }
        else
        {
            deathTimer = 0f;
        }
    }
}
