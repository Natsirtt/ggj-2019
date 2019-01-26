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

    public void TakeJob(JobDispatcher.Job job)
    {
        // trigger animation
        gameObject.GetComponent<Pathfollowing>().MoveToLocation(job.Coordinates);
        Job = job;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        IsIdle = true;
    }

    // Update is called once per frame
    void Update()
    {
        // somehow check if you are at the jobsite and once done remove the job
    }
}
