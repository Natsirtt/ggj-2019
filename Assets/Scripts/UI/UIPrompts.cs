﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPrompts : MonoBehaviour
{
    public GameObject TextPrompt;
    private UnityEngine.UI.Text uiText;
    public float duration;
    private float coolDown = 0f;
    // Start is called before the first frame update
    void Awake()
    {
        uiText = TextPrompt.GetComponent<UnityEngine.UI.Text>();
    }
    public void SetText(string text)
    {
        uiText.text = text;
        coolDown = 0f;
    }
    void Update()
    {
        if (uiText.text == "") return; 
        coolDown += Time.deltaTime;
        if (coolDown >= duration)
        {
            uiText.text = "";
        }
    }
}
