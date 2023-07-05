using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.Tests{


public class ChangeRatio : MonoBehaviour
{
    [SerializeField]
    private GameObject _Btn;
    [SerializeField]
    private GameObject[] Btns;

    public void openRatio(){
        if(!_Btn.activeSelf){
            _Btn.SetActive(true);
        }
        else{
            _Btn.SetActive(false);
        }
    }
    public void changeRatio(int ratio){
        GameObject obj = GameObject.Find("AR Camera");
        obj.GetComponent<Test_ScreenShot>().CameraRatio = ratio;
        _Btn.SetActive(false);
    }
}
}
