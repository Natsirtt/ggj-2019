using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

public class PeopleTrackerUI : MonoBehaviour
{
    private World world;
    private UI.Text displayText;
    // Start is called before the first frame update
    void Start()
    {
        world = World.Get();
        displayText = gameObject.GetComponent<UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = world.Workers.Count.ToString();
    }
}
