using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField]
    private int burnRatePerSecond = 0;

    [SerializeField]
    private int burnRatePerSecondIncrease = 1;

    [SerializeField]
    private int radiusOfInfluence = 0;

    [SerializeField]
    private int radiusOfInfluenceIncrease = 5;

    private int currentBurnRatePerSecond;
    private int currentRadiusOfInfluence;

    private float burnProgress;
    private Inventory globalInventory;
    // Start is called before the first frame update
    void Start()
    {
        World world = World.Get();
        globalInventory = world.GlobalInventory;
        burnProgress = 0f;
        radiusOfInfluence = 
        currentBurnRatePerSecond = burnRatePerSecond;
    }

    // Update is called once per frame
    void Update()
    {
        burnProgress += Time.deltaTime;
        if (burnProgress >= 1f)
        {
            if (globalInventory.CurrentWood < burnRatePerSecond)
            {
                // Warn the player, dim down the fire?
                Shrink();
                return;
            }
            globalInventory.RemoveWood(burnRatePerSecond);
            burnProgress = 0f;
        }
    }

    public int CurrentRadiusOfInfluence
    {
        get { return currentRadiusOfInfluence; }
        set { currentRadiusOfInfluence = value;  }

    }

    public void Deactivate()
    {
        // TODO change the display as well!
        burnProgress = 0f;
        radiusOfInfluence = 0;
        burnRatePerSecond = 0;
    }

    public void Activate()
    {
        burnProgress = 0f;
        currentRadiusOfInfluence = radiusOfInfluence;
        currentBurnRatePerSecond = burnRatePerSecond;
    }

    public void Feed()
    {
        currentRadiusOfInfluence += radiusOfInfluenceIncrease;
        currentBurnRatePerSecond += burnRatePerSecondIncrease;

    }

    public void Shrink()
    {
        currentRadiusOfInfluence -= radiusOfInfluenceIncrease;
        currentBurnRatePerSecond -= burnRatePerSecondIncrease;
        if (currentBurnRatePerSecond <= 0)
        {
            Deactivate();
        }
    }

}
