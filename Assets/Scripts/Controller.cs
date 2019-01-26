using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject debugObject;

    void Update()
    {
        if(Input.GetButtonDown("PlaceCampFire"))
        {
            Vector2 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            World.Get().SpawnCampFire(mousePosInWorld);

            GameObject.Instantiate<GameObject>(debugObject, new Vector3(mousePosInWorld.x, mousePosInWorld.y, 0), new Quaternion());
        }
    }
}
