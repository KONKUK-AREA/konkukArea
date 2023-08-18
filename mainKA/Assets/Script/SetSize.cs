using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSize : MonoBehaviour
{
    private RectTransform canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(canvas.rect.width, canvas.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
