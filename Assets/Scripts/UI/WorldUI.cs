using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

public class WorldUI : MonoBehaviour
{
    private Inventory globalInventory;
    private UI.Text displayText;
    // Start is called before the first frame update
    void Start()
    {
        World world = World.Get();
        globalInventory = world.GlobalInventory;
        displayText = gameObject.GetComponent<UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = "Current Wood: " + globalInventory.CurrentWood.ToString();
    }
}
