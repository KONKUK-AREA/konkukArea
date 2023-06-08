using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    public GameObject logoUI;
    public GameObject CameraView;
    public GameObject WaitQR;
    public GameObject KUMSG;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(beginProgram());
    }
    
    IEnumerator beginProgram()
    {
        yield return new WaitForSeconds(2f);
        logoUI.SetActive(false);
        CameraView.SetActive(true);
        WaitQR.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (WaitQR.activeSelf)
            {
                WaitQR.SetActive(false);
                KUMSG.SetActive(true);
            }
            else if (KUMSG.activeSelf)
            {
                KUMSG.SetActive(false);
            }

           
        }
    }
}
