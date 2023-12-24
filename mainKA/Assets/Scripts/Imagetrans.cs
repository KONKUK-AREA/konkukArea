using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Imagetrans : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject _camera;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = _camera.transform.position + new Vector3(0, 1, 4);
    }
}
