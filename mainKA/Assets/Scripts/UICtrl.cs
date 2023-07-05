using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    public GameObject logoUI;
    public GameObject WaitQR;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(beginProgram());
    }
    
    IEnumerator beginProgram()
    {
        yield return new WaitForSeconds(2f);
        logoUI.SetActive(false);
        WaitQR.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {

           
        
    }
}
