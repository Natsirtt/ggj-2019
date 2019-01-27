﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    Vector3? DragCameraInputLocation;
    Vector3? DragCameraStartLocation;

    public bool BlockGameplayInputs { get; set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (BlockGameplayInputs && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Game");
            return;
        }

        Vector2 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetButtonDown("PlaceCampFire"))
        {
            if (BlockGameplayInputs)
            {
                return;
            }

            World w = World.Get();
            if (w.Tiles[w.GetGridLocation(mousePosInWorld)].TileType == World.Tile.Type.Grass)
            {
                GameObject fire = w.GetClosestFire(mousePosInWorld);
                JobDispatcher jobScript = fire.GetComponent<JobDispatcher>();
                jobScript.QueueJob(w.GetGridLocation(mousePosInWorld), JobDispatcher.Job.Type.Expedition);
            }
            else if (w.Tiles[w.GetGridLocation(mousePosInWorld)].TileType == World.Tile.Type.Hearth)
            {
                if (w.GlobalInventory.CurrentWood >= w.GenerationParameters.resources.hearthFeedingAmount)
                {
                    w.GlobalInventory.RemoveWood(w.GenerationParameters.resources.hearthFeedingAmount);
                    w.Hearth.GetComponent<Fire>().Feed();
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
