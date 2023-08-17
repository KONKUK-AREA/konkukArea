using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSize : MonoBehaviour
{
    // Start is called before the first frame update
    Canvas canvas;
    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        this.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
