using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private int StartingWood;

    private int currentWood;
    public int CurrentWood
    {
        get { return currentWood; }
        set { currentWood = value;  }
    }

    public void addWood(int amount)
    {
        currentWood += amount;
    }

    public void removeWood(int amount)
    {
        currentWood -= amount;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentWood = StartingWood;   
    }
}
