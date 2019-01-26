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

        public Job(Vector2Int coordinates, Type type)
        {
            Coordinates = coordinates;
            JobType = type;
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
                expeditionQueue.Enqueue(new Job(jobLocation, type));
                break;
        }
    }

    public void QueueJob(Job job)
    {
        chopWoodQueue.Enqueue(job);
    }


    // Start is called before the first frame update
    void Start()
    {
        world = World.Get();
        globalInventory = world.GlobalInventory;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Job job in expeditionQueue)
        {
            GameObject worker = world.GetClosestIdleWorker(job.Coordinates);
            if (worker != null)
            {
                worker.GetComponent<AgentJobHandler>().TakeJob(job);
            }
        }
        foreach (Job job in chopWoodQueue)
        {
            GameObject worker = world.GetClosestIdleWorker(job.Coordinates);
            if (worker != null)
            {
                worker.GetComponent<AgentJobHandler>().TakeJob(job);
            }
        }
    }
}