using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TouchGalleryUI : MonoBehaviour
{
    public GameObject Btns;
    private bool isBtn = false;
    private IEnumerator coroutine;
    private GraphicRaycaster gr;
    // Start is called before the first frame update
    void Start()
    {
        gr=GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        coroutine = DelayDestroyUI();
    }

    void StartDelayCoroutine()
    {
        if (coroutine == null)
        {
            coroutine = DelayDestroyUI();
            StartCoroutine(coroutine);
        }
    }
    void StopDelayCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
    // Update is called once per frame
    void Update()
    {

        if (Input.touchCount == 0) return;
        Touch touch = Input.touches[0];
        var ped = new PointerEventData(null);
        List<RaycastResult> results = new List<RaycastResult>();
        ped.position = touch.position;
        gr.Raycast(ped, results);

        if (touch.phase == TouchPhase.Began)
        {
            if (results[0].gameObject.CompareTag("GalleryBack")){
                isBtn = !isBtn;
                Btns.SetActive(isBtn);

                StopDelayCoroutine();
                if (Btns.activeSelf)
                {
                    StartDelayCoroutine();
                }
            }
        }
    }
    IEnumerator DelayDestroyUI()
    {
        yield return new WaitForSeconds(2f);
        Btns.SetActive(false);
        isBtn = false;
    }
}
