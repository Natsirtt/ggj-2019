using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        outPoint = PathPoints.Last();
        if ((queryPosition - outPoint).magnitude < 1.0f)
        {
            PathPoints.RemoveAt(PathPoints.Count - 1);
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
    public static bool BuildPath(Dictionary<Vector2Int, World.Tile> inGrid, Vector2 startPos, Vector2 endPos, ref Path outPath)
    {
        Vector2Int startPosInt = World.GetGridLocation(startPos);
        Vector2Int endPosInt = World.GetGridLocation(endPos);
        if (!inGrid.ContainsKey(startPosInt) || !inGrid.ContainsKey(endPosInt))
            return false;

        World.Tile start = inGrid[startPosInt];
        World.Tile end = inGrid[endPosInt];

        List<World.Tile> open = new List<World.Tile>();
        List<World.Tile> closed = new List<World.Tile>();
        List<World.Tile> adjancencies = new List<World.Tile>();

        World.Tile current = start;

        open.Add(current);
        
        while(open.Count != 0 && !closed.Exists( x => x.Coordinates == end.Coordinates) && closed.Count < 1000)
        {
            current = open[0];
            open.Remove(current);
            closed.Add(current);
            adjancencies = AStar.GetAdjacentNodes(inGrid, current);

            foreach(World.Tile tile in adjancencies)
            {
                if(!closed.Contains(tile) && tile.IsTraversable())
                {
                    if(!open.Contains(tile))
                    {
                        tile.Parent = current;
                        tile.DistanceToTarget = (tile.Coordinates - endPosInt).magnitude;
                        tile.Cost = 1.0f + tile.Parent.Cost;
                        open.Add(tile);
                        open = open.OrderBy(t => t.F).ToList<World.Tile>();
                    }
                }
            }
        }

        if(!closed.Exists(x => x.Coordinates == endPosInt))
        {
            return false;
        }

        World.Tile curr = closed[closed.IndexOf(current)];
        while(curr != null && curr.Parent != start)
        {
            outPath.PathPoints.Add( World.GetWorldLocation(curr.Coordinates));
            curr = curr.Parent;
        }

        return true;
    }

    private static List<World.Tile> GetAdjacentNodes(Dictionary<Vector2Int, World.Tile> inGrid, World.Tile tile)
    {
        List<World.Tile> temp = new List<World.Tile>();

        Vector2Int coordinates = tile.Coordinates;

        if (inGrid.ContainsKey(coordinates + Vector2Int.up))
        {
            temp.Add(inGrid[coordinates + Vector2Int.up]);
        }

        if (inGrid.ContainsKey(coordinates + Vector2Int.down))
        {
            temp.Add(inGrid[coordinates + Vector2Int.down]);
        }

        if (inGrid.ContainsKey(coordinates + Vector2Int.left))
        {
            temp.Add(inGrid[coordinates + Vector2Int.left]);
        }

        if (inGrid.ContainsKey(coordinates + Vector2Int.right))
        {
            temp.Add(inGrid[coordinates + Vector2Int.right]);
        }

        return temp;
    }
}

public class Pathfollowing : MonoBehaviour
{
    Path CurrentPath = new Path();

    private void Start()
    {
        InvokeRepeating("MoveToRandomLocationInSquare", 2.0f, 2.0f);
    }

    void MoveToRandomLocationInSquare()
    {
        if(!CurrentPath.HasPath())
            MoveToLocation(new Vector2(Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)) + new Vector2(transform.position.x, transform.position.y));
    }

    public void MoveToLocation(Vector2 location)
    {
        if (!AStar.BuildPath(World.Get().Tiles, transform.position, location, ref CurrentPath))
        {
            Debug.LogWarning("No path could be built for agent: " + gameObject.name + " at location " + location.ToString());
        }
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
                agentMovement.AddMovementInput(vecTowards.magnitude > 1.0f ? vecTowards.normalized : vecTowards);
            }

            for(int i = 0; i < CurrentPath.PathPoints.Count - 1; ++i)
            {
                Debug.DrawLine(CurrentPath.PathPoints[i], CurrentPath.PathPoints[i+1], Color.red);
            }
        }
    }
}
