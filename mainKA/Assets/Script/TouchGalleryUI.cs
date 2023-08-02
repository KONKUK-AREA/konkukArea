using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TouchGalleryUI : MonoBehaviour
{
    public GameObject Btns;
    private bool isBtn = false;
    private IEnumerator coroutine;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.touches[0];
        if (touch.phase == TouchPhase.Began)
        {
            isBtn = !isBtn;
            Btns.SetActive(isBtn);
            StopCoroutine(coroutine);
            if (Btns.activeSelf)
            {
                coroutine = DelayDestroyUI();
                StartCoroutine(coroutine);
            }
        }
    }
    IEnumerator DelayDestroyUI()
    {
        yield return new WaitForSeconds(3f);
        Btns.SetActive(false);
        isBtn = false;
    }
}
