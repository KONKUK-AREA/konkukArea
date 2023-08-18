using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KUFirstScale : MonoBehaviour
{
    public float scale;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = Vector3.one * scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
