using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private int StartingWood = 0;

    private int currentWood = 0;
    public int CurrentWood
    {
        get { return currentWood; }
        set { currentWood = value;  }
    }

    public void AddWood(int amount)
    {
        currentWood += amount;
    }

    public void RemoveWood(int amount)
    {
        currentWood -= amount;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentWood = StartingWood;
    }
}
