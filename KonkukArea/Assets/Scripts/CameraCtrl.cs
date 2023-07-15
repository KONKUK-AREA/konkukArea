using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using JetBrains.Annotations;
using System;

public class CameraCtrl : MonoBehaviour
{
    WebCamTexture camTexture;
    public RawImage cameraViewImage;
    // Start is called before the first frame update
    void Start()
    {
        CameraOn();
    }

    private void CameraOn()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if(WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No Camera!");
            return;
        }
        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;
        //후면 카메라 찾기
        for(int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex= i;
                break;
            }
        }
        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);

            camTexture.requestedFPS = 60;

            cameraViewImage.texture = camTexture;
            camTexture.Play();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
