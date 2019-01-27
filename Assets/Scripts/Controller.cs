using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    System.Nullable<Vector3> DragCameraInputLocation;
    System.Nullable<Vector3> DragCameraStartLocation;

    void Update()
    {
        Vector2 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetButtonDown("PlaceCampFire"))
        {
            World w = World.Get();
            Vector2Int gridLocation = w.GetGridLocation(mousePosInWorld);
            if (w.Tiles[gridLocation].TileType == World.Tile.Type.Grass)
            {
                GameObject fire = w.GetClosestFire(mousePosInWorld);
                Vector2Int closestFireGridLocation = w.GetGridLocation(fire.transform.position);
                int distance = World.GetManhattanDistance(gridLocation, closestFireGridLocation);
                int cost = w.GenerationParameters.resources.expeditionWoodCostPerTile * distance;
                
                if (w.GlobalInventory.CurrentWood > cost) {
                    w.SetTileType(gridLocation, World.Tile.Type.ExpeditionSite);
                    JobDispatcher jobScript = fire.GetComponent<JobDispatcher>();
                    jobScript.QueueJob(gridLocation, JobDispatcher.Job.Type.Expedition);
                    w.GlobalInventory.RemoveWood(cost);
                }
            }
        }

        if(Input.GetButtonDown("DragCamera"))
        {
            DragCameraInputLocation = Input.mousePosition;
            DragCameraStartLocation = Camera.main.transform.position;
        }

        if(DragCameraInputLocation != null)
        {
            Vector2 dragV = DragCameraInputLocation.Value - Input.mousePosition;
            dragV *= Camera.main.orthographicSize;
            float ratio = Camera.main.scaledPixelWidth / Camera.main.scaledPixelHeight;
            dragV.y *= ratio;
            dragV /= 200.0f;

            float oldZ = Camera.main.transform.position.z;
            Camera.main.transform.position = new Vector3(dragV.x, dragV.y, 0) + DragCameraStartLocation.Value;
        }

        if(Input.GetButtonUp("DragCamera"))
        {
            DragCameraInputLocation = null;
            DragCameraStartLocation = null;
        }

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollAmount) > 0.0f)
        {
            Camera.main.orthographicSize -= scrollAmount;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 80, 700);
        }
    }
}
