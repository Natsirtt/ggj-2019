using UnityEngine;

public class World : MonoBehaviour
{
    private Inventory worldInventory;
    private static World worldInstance = null;
    public static World Get()
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

    public Inventory GlobalInventory
    {
        get { return worldInventory; }
    }

    void Awake()
    {
        worldInventory = gameObject.AddComponent<Inventory>();
    }

    void Update()
    {
    }
}
