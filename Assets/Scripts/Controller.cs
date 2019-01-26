using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    void Update()
    {
        if(Input.GetButtonDown("PlaceCampFire"))
        {
            Vector2 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            World.Get().SpawnCampFire(mousePosInWorld);
        }
    }
}
