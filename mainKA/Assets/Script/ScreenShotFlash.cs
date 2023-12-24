using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShotFlash : MonoBehaviour
{
    public float duration = 0.3f;
    private Image _image;
    private float _currentAlpha = 1f;
    private GameObject flash;
    private void Awake()
    {
        flash = GameObject.FindWithTag("Flash");
        _image = flash.GetComponent<Image>();
    }
    private void Update()
    {
        Color col = _image.color;
        col.a = _currentAlpha;
        _image.color = col;

        _currentAlpha -= Time.unscaledDeltaTime / duration;

        if (_currentAlpha < 0f)
            _image.enabled = false;
    }

    public void Show()
    {
        _currentAlpha = 1f;
        _image.enabled = true;
    }
}
