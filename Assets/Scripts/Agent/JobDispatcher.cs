using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobDispatcher : MonoBehaviour
{
    public class Job
    {
        static public float ExpeditionCostModifier { get; set; }
        public enum Type
        {
            Chop,
            Expedition
        }

        public Vector2Int Coordinates { get; private set; }
        public Type JobType { get; set; }

        public int NumWorkers { get; set; }
        public int NumWorkersAssigned { get; set; }
        public int RequiredWorkers { get; set; }

        public Job(Vector2Int coordinates, Type type, int requiredWorkers = 1)
        {
            Coordinates = coordinates;
            RequiredWorkers = requiredWorkers;
            NumWorkers = 0;
            NumWorkersAssigned = 0;
            JobType = type;
        }

        public void Arrived()
        {
            NumWorkers += 1;
        }

        public bool IsReady()
        {
            return NumWorkers == RequiredWorkers;
        }

        public float Duration()
        {
            return 5f;
        }
    }


    #region Job Handler private members

    private Queue<Job> expeditionQueue;
    private Queue<Job> chopWoodQueue;
    private Inventory globalInventory;
    private World world;
    #endregion

    public void QueueJob(Vector2Int jobLocation, Job.Type type)
    {
        switch (type)
        {
            case Job.Type.Chop:
                chopWoodQueue.Enqueue(new Job(jobLocation, type));
                break;
            case Job.Type.Expedition:
                expeditionQueue.Enqueue(new Job(jobLocation, type, 5));
                break;
        }
    }

    public Job GetNearestJob(Vector2Int gridPosition)
    {
        if (expeditionQueue.Count > 0)
        {
            return expeditionQueue.Peek();
        }
        return chopWoodQueue.Dequeue();
        /*int closestDistance = 999999;
        Job closestJob = null;
        foreach (Job job in expeditionQueue)
        {
            int distance = World.GetManhattanDistance(gridPosition, job.Coordinates);
            if (distance < closestDistance)
            {
                closestJob = job;
                closestDistance = distance;
            }
        }
        return closestJob;*/
    }

    public void dequeueExpedition()
    {
        expeditionQueue.Dequeue();
    }

    public void ClearJobs()
    {
        chopWoodQueue.Clear();
    }

    public bool HasJobs()
    {
        return chopWoodQueue.Count > 0 || expeditionQueue.Count > 0;
    }

    public void QueueJob(Job job)
    {
        chopWoodQueue.Enqueue(job);
    }

    public Queue<Job> chopJobs()
    {
        return chopWoodQueue;
    }

    public int Count
    {
        get
        {
            return chopWoodQueue.Count + expeditionQueue.Count;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        world = World.Get();
        globalInventory = world.GlobalInventory;
        expeditionQueue = new Queue<Job>();
        chopWoodQueue = new Queue<Job>();
    }

}