using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<Vector2> PathPoints = new List<Vector2>();

    public bool HasPath()
    {
        return PathPoints.Count > 0;
    }

    public static Vector2 GetClosestPointOnSegment(Vector2 A, Vector2 B, Vector2 P)
    {
        float l2 = (A - B).magnitude;
        if (Mathf.Approximately(l2, 0.0f))
            return A;

        float t = Mathf.Max(0.0f, Mathf.Min(1.0f, Vector2.Dot(P - A, B - A) / l2));
        Vector2 projection = A + t * (B - A);
        return projection;
    }

    public bool GetNextPoint(Vector2 queryPosition, out Vector2 outPoint)
    {
        outPoint = Vector2.zero;
        if (!HasPath())
            return false;

        outPoint = PathPoints[0];
        if ((queryPosition - outPoint).magnitude < 0.1f)
        {
            PathPoints.RemoveAt(0);
            return GetNextPoint(queryPosition, out outPoint);
        }

        return true;
    }

    // IMPLEMENT LATER MAYBE
    public Vector2 GetClosestPointOnPath(Vector2 queryLocation, float forwardOffset)
    {
        Vector2 result = Vector2.zero;

        // TODO LATER
        float closestDistance = float.MaxValue;
        for (int i = 0; i < PathPoints.Count - 1; ++i)
        {
            Vector2 segment = PathPoints[i + 1] - PathPoints[i];
            Vector2 finalQueryLocation = queryLocation + segment.normalized * forwardOffset;
            Vector2 closestPointOnSegment = GetClosestPointOnSegment(PathPoints[i], PathPoints[i + 1], finalQueryLocation);
            Vector2 vecTowardsPoint = closestPointOnSegment - finalQueryLocation;
            if (vecTowardsPoint.sqrMagnitude < closestDistance)
            {
                result = closestPointOnSegment;
                closestDistance = vecTowardsPoint.sqrMagnitude;
            }
        }

        return result;
    }

}

public class AStar
{
    public static bool BuildPath(Dictionary<Vector2Int, World.Tile> inGrid, Vector2 start, Vector2 end, ref Path outPath)
    {


        return false;
    }
}

public class Pathfollowing : MonoBehaviour
{
    Path CurrentPath = new Path();

    private void Start()
    {
        CurrentPath.PathPoints.Add(new Vector2(-10, 10));
        CurrentPath.PathPoints.Add(new Vector2(-10, -10));
        CurrentPath.PathPoints.Add(new Vector2(10, -10));
        CurrentPath.PathPoints.Add(new Vector2(10, 10));
        CurrentPath.PathPoints.Add(new Vector2(-10, 10));
    }

    public void MoveToLocation(Vector2 location)
    {

    }

    void Update()
    {
        AgentMovement agentMovement = GetComponent<AgentMovement>();
        if(agentMovement && CurrentPath.HasPath())
        {

            Vector2 closestPointOnPath;// CurrentPath.GetClosestPointOnPath(transform.position, 0.1f);
            if (CurrentPath.GetNextPoint(transform.position, out closestPointOnPath))
            {
                Debug.DrawLine(transform.position, closestPointOnPath);
                Vector2 vecTowards = closestPointOnPath - new Vector2(transform.position.x, transform.position.y);
                agentMovement.AddMovementInput(vecTowards.normalized);
            }
        }
    }
}
