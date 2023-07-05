using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotCtrl : MonoBehaviour
{
    public void CaptureScreen()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string fileName = timestamp + ".png";
        if (Application.platform == RuntimePlatform.Android)
        {
            CaptureScreenForMobile(fileName);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            CaptureScreenForMobile(fileName);
        }
        else
        {
            CaptureScreenForPC(fileName);
        }
    }
    private void CaptureScreenForPC(string fileName)
    {
        ScreenCapture.CaptureScreenshot("~/Downloads/" + fileName);
        Debug.Log("PC");
    }
    private void CaptureScreenForMobile(string fileName)
    {

    }
}
