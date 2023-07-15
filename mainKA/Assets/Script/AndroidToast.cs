using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//안드로이드 토스트 메시지 표시 싱글톤
public class AndroidToast : MonoBehaviour
{
    #region Singleton

    //싱글톤 인스턴스 Getter
    public static AndroidToast I
    {
        get
        {
            if (instance == null)    // 체크 1 : 인스턴스가 없는 경우
                CheckExsistence();

            return instance;
        }
    }

    // 싱글톤 인스턴스
    private static AndroidToast instance;

    // 싱글톤 인스턴스 존재 여부 확인 (체크 2)
    private static void CheckExsistence()
    {
        // 싱글톤 검색
        instance = FindObjectOfType<AndroidToast>();

        // 인스턴스 가진 오브젝트가 존재하지 않을 경우, 빈 오브젝트를 임의로 생성하여 인스턴스 할당
        if (instance == null)
        {
            // 빈 게임 오브젝트 생성
            GameObject container = new GameObject("AndroidToast Singleton Container");

            // 게임 오브젝트에 클래스 컴포넌트 추가 후 인스턴스 할당
            instance = container.AddComponent<AndroidToast>();
        }
    }

    private void CheckInstance()
    {
        // 싱글톤 인스턴스가 존재하지 않았을 경우, 본인으로 초기화
        if (instance == null)
            instance = this;

        // 싱글톤 인스턴스가 존재하는데, 본인이 아닐 경우, 스스로(컴포넌트)를 파괴
        if (instance != null && instance != this)
        {
            Debug.Log("이미 AndroidToast 싱글톤이 존재하므로 오브젝트를 파괴합니다.");
            Destroy(this);

            // 만약 게임 오브젝트에 컴포넌트가 자신만 있었다면, 게임 오브젝트도 파괴
            var components = gameObject.GetComponents<Component>();

            if (components.Length <= 2)
                Destroy(gameObject);
        }
    }

    private void Awake()
    {
        CheckInstance();
    }

    #endregion // ==================================================================

    public enum ToastLength
    {
        //약 2.5초
        Short,
        //약 4초
        Long
    };

#if UNITY_EDITOR
    private float __editorGuiTime = 0f;
    private string __editorGuiMessage;

#elif UNITY_ANDROID

    private AndroidJavaClass _unityPlayer;
    private AndroidJavaObject _unityActivity;
    private AndroidJavaClass _toastClass;

    private void Start()
    {
        _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        _unityActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        _toastClass = new AndroidJavaClass("android.widget.Toast");
    }
#endif

    //안드로이드 토스트 메시지 표시하기
    [System.Diagnostics.Conditional("UNITY_ANDROID")]
    public void ShowToastMessage(string message, ToastLength length = ToastLength.Short)
    {
#if UNITY_EDITOR
        __editorGuiTime = length == ToastLength.Short ? 2.5f : 4f;
        __editorGuiMessage = message;

#elif UNITY_ANDROID
        if (_unityActivity != null)
        {
            _unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = _toastClass.CallStatic<AndroidJavaObject>("makeText", _unityActivity, message, (int)length);
                toastObject.Call("show");
            }));
        }
#endif
    }

#if UNITY_EDITOR
    /* 유니티 에디터 IMGUI를 통해 토스트 메시지 표시 모방하기 */

    private GUIStyle toastStyle;
    private void OnGUI()
    {
        if (__editorGuiTime <= 0f) return;

        float width = Screen.width * 0.5f;
        float height = Screen.height * 0.08f;
        Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.8f, width, height);

        if (toastStyle == null)
        {
            toastStyle = new GUIStyle(GUI.skin.box);
            toastStyle.fontSize = 36;
            toastStyle.fontStyle = FontStyle.Bold;
            toastStyle.alignment = TextAnchor.MiddleCenter;
            toastStyle.normal.textColor = Color.white;
        }

        GUI.Box(rect, __editorGuiMessage, toastStyle);
    }
    private void Update()
    {
        if (__editorGuiTime > 0f)
            __editorGuiTime -= Time.unscaledDeltaTime;
    }
#endif
}