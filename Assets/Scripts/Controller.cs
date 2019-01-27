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
            if (w.Tiles[w.GetGridLocation(mousePosInWorld)].TileType == World.Tile.Type.Grass)
            {
                w.SetTileType(w.GetGridLocation(mousePosInWorld), World.Tile.Type.ExpeditionSite);
                GameObject fire = w.GetClosestFire(mousePosInWorld);
                JobDispatcher jobScript = fire.GetComponent<JobDispatcher>();
                jobScript.QueueJob(w.GetGridLocation(mousePosInWorld), JobDispatcher.Job.Type.Expedition);
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
