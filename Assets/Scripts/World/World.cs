using UnityEngine;

public class World : MonoBehaviour
{
    private static World worldInstance = null;
    static World Get()
    {
        if (worldInstance == null)
        {
            worldInstance = FindObjectOfType<World>();
            if (worldInstance == null)
            {
                var newWorld = new GameObject("World");
                worldInstance = newWorld.AddComponent<World>();
            }
        }
        return worldInstance;
    }

    void Start()
    {   
    }

    void Update()
    {
    }
}
