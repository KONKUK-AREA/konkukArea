using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
namespace Rito.Tests{


public class ChangeRatio : MonoBehaviour
{
    [SerializeField]
    private GameObject _Btn;
    [SerializeField]
    private GameObject[] Btns;
    [SerializeField]
    private RectTransform Selected;
        [SerializeField]
        private RectTransform TopFrame;
        [SerializeField]
        private RectTransform BottomFrame;
        GameObject button;
        Vector2 startSelectedPos;
        GameObject cameraFrame;
        GameObject canvas;
        private void Start()
        {
            startSelectedPos = Selected.anchoredPosition;
        }
        public void SetSetting()
        {
            cameraFrame = GameObject.Find("CameraFrame");
            canvas = GameObject.Find("Canvas");
            cameraFrame.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;
            changeRatio(0);
        }
        public void openRatio(){
        button = EventSystem.current.currentSelectedGameObject;
        if(!_Btn.activeSelf){
            _Btn.SetActive(true);
                button.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else{
            _Btn.SetActive(false);
                button.transform.localEulerAngles = new Vector3(0, 0, 0);

            }
        }
    public void changeRatio(int ratio){
        GameObject obj = GameObject.Find("AR Camera");
        obj.GetComponent<Test_ScreenShot>().CameraRatio = ratio;
        Vector2 setPos = new Vector2(200 * ratio, 0);
            Selected.anchoredPosition = startSelectedPos + setPos;
            float width, height=0;
            width = cameraFrame.GetComponent<RectTransform>().rect.width;

            switch (ratio)
            {
                case 0: // 16:9
                    height = (width * (16f / 9f)); break;
                case 1: // 4: 3
                    height = (width * (4f / 3f)); break;
                case 2: // Á¤¹æ
                    height = width; break;
                case 3: // full
                    height = cameraFrame.GetComponent<RectTransform>().rect.height; break;
            }
            float PosY = (cameraFrame.GetComponent<RectTransform>().rect.height - height);
            changeTxtColor(ratio);
            Rect area = new Rect(0f, PosY / 4f, width, height);
            Debug.Log(area);
            TopFrame.sizeDelta = new Vector2(width, PosY);
            TopFrame.anchoredPosition = new Vector2(0, 0);
            BottomFrame.sizeDelta = new Vector2(width, PosY);
            BottomFrame.anchoredPosition = new Vector2(0, 0);

    }
    private void changeTxtColor(int num)
        {
            foreach(GameObject obj in Btns)
            {
                obj.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            }
            Btns[num].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

}
}
