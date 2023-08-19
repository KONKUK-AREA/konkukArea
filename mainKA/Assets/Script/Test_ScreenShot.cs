using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;



//Window, Android, iOS 환경에 따라 추가 및 수정하기
#if UNITY_ANDROID
using UnityEngine.Android;
#endif



namespace Rito.Tests
{

    public class Test_ScreenShot : MonoBehaviour
    {
        /***********************************************************************
        *                               Public Fields
        ***********************************************************************/
        #region .
        public Button screenShotButton;          // 전체 화면 캡쳐
        public Button screenShotWithoutUIButton; // UI 제외 화면 캡쳐
        public Button readAndShowButton; // 저장된 경로에서 스크린샷 파일 읽어와서 이미지에 띄우기
        public Button openGallery;      // 갤러리 앱 열기
        public Image imageToShow;        // 띄울 이미지 컴포넌트
        public Image galleryToShow;
        public GameObject flashObject;
        public int CameraRatio = 0; // 카메라 비율 관리 변수
        public string folderName = "ScreenShots";
        public string fileName = "MyScreenShot";
        public string extName = "png";
        private bool _willTakeScreenShot = false;
        private PathStack ShotImages = new PathStack();
        #endregion
        /***********************************************************************
        *                               Fields & Properties
        ***********************************************************************/
        #region .
        private Texture2D _imageTexture; // imageToShow의 소스 텍스쳐
        [SerializeField]
        private Texture2D _galleryTexture;
        [SerializeField]
        private GameObject Gallery;
        [SerializeField]
        private GameObject Menu;
        [SerializeField]
        private GameObject Filter;
        [SerializeField]
        private Image[] FilterImgs;
        [SerializeField]
        private Sprite[] FilterSprites;
        private Camera _ARcamera;
        private GameObject canvas;
        private bool isFinishCapture = false;
        private string RootPath
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return Application.dataPath;
#elif UNITY_ANDROID
                return $"/storage/emulated/0/DCIM/{Application.productName}/";
                //return Application.persistentDataPath;
#endif
            }
        }
        private string FolderPath => $"{RootPath}/{folderName}";
        private string TotalPath => $"{FolderPath}/{fileName}_{DateTime.Now.ToString("MMdd_HHmmss")}.{extName}";

        private string lastSavedPath;

        #endregion

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            canvas = GameObject.Find("Canvas");
            screenShotWithoutUIButton.onClick.AddListener(StartFlash);
            screenShotButton.onClick.AddListener(StartFlash);

            screenShotButton.onClick.AddListener(TakeScreenShotFull);

            //screenShotWithoutUIButton.onClick.AddListener(TakeScreenShotWithoutUI);
            screenShotWithoutUIButton.onClick.AddListener(CaptureOnlyExcludeUI);
            readAndShowButton.onClick.AddListener(ReadScreenShotAndShow);
            openGallery.onClick.AddListener(imageOpen);
            _ARcamera = GetComponent<Camera>();
            Gallery.GetComponentInChildren<Image>().rectTransform.localScale = canvas.GetComponent<RectTransform>().localScale;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("GalleryBack"))
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
            }
#if UNITY_ANDROID
            CheckAndroidPermissionAndDo(Permission.ExternalStorageWrite, () => { });
            CheckAndroidPermissionAndDo(Permission.ExternalStorageRead, () => { });
#endif
        }
        #endregion
        /***********************************************************************
        *                               Button Event Handlers
        ***********************************************************************/
        #region .
        // UI 포함 전체 화면 캡쳐
        private void TakeScreenShotFull()
        {
#if UNITY_ANDROID
            CheckAndroidPermissionAndDo(Permission.ExternalStorageWrite, () => StartCoroutine(TakeScreenShotRoutine()));
#else
            StartCoroutine(TakeScreenShotRoutine());
#endif
        }

        // UI 미포함, 현재 카메라가 렌더링하는 화면만 캡쳐
        private void TakeScreenShotWithoutUI()
        {
#if UNITY_ANDROID
            CheckAndroidPermissionAndDo(Permission.ExternalStorageWrite, () => _willTakeScreenShot = true);
#else
            _willTakeScreenShot = true;
#endif
        }

        // 스크린샷 띄워주기
        private void ReadScreenShotAndShow()
        {
#if UNITY_ANDROID
            CheckAndroidPermissionAndDo(Permission.ExternalStorageRead, () => ReadScreenShotFileAndShow(imageToShow));
#else
            ReadScreenShotFileAndShow(imageToShow);
#endif
        }
        string OpenPath;

        // 갤러리 앱 열기
        private void OpenGalleryApp()
        {

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass intentStaticClass = new AndroidJavaClass("android.content.Intent");
            string actionView = intentStaticClass.GetStatic<string>("ACTION_VIEW");
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "content://media/internal/images/media");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", actionView, uriObject);
            unityActivity.Call("startActivity", intent);



        }
        #endregion
        /***********************************************************************
        *                               Methods
        ***********************************************************************/
        #region .

        // UI 포함하여 현재 화면에 보이는 모든 것 캡쳐
        private IEnumerator TakeScreenShotRoutine()
        {
            yield return new WaitForEndOfFrame();
            CaptureScreenAndSave();
        }

        // UI 제외하고 현재 카메라가 렌더링하는 모습 캡쳐
        private void OnPostRender()
        {
            if (_willTakeScreenShot)
            {
                _willTakeScreenShot = false;
                //CaptureScreenAndSave();
                //CaptureOnlyExcludeUIMain();
            }
        }


#if UNITY_ANDROID
        //안드로이드 - 권한 확인하고, 승인시 동작 수행하기
        private void CheckAndroidPermissionAndDo(string permission, Action actionIfPermissionGranted)
        {
            // 안드로이드 : 저장소 권한 확인하고 요청하기
            if (Permission.HasUserAuthorizedPermission(permission) == false)
            {
                PermissionCallbacks pCallbacks = new PermissionCallbacks();
                pCallbacks.PermissionGranted += str => Debug.Log($"{str} 승인");
                pCallbacks.PermissionGranted += str => AndroidToast.I.ShowToastMessage($"{str} 권한을 승인하셨습니다.");
                pCallbacks.PermissionGranted += _ => actionIfPermissionGranted(); // 승인 시 기능 실행

                pCallbacks.PermissionDenied += str => Debug.Log($"{str} 거절");
                pCallbacks.PermissionDenied += str => AndroidToast.I.ShowToastMessage($"{str} 권한을 거절하셨습니다.");

                pCallbacks.PermissionDeniedAndDontAskAgain += str => Debug.Log($"{str} 거절 및 다시는 보기 싫음");
                pCallbacks.PermissionDeniedAndDontAskAgain += str => AndroidToast.I.ShowToastMessage($"{str} 권한을 격하게 거절하셨습니다.");

                Permission.RequestUserPermission(permission, pCallbacks);
            }
            else
            {
                actionIfPermissionGranted(); // 바로 기능 실행
            }
        }
#endif

        //스크린샷을 찍고 경로에 저장하기
        private void CaptureScreenAndSave()
        {
            int width, height = 0; // 사진 가로, 세로
            width = Screen.width;

            switch (CameraRatio)
            {
                case 0: // 16:9
                    height = (int)(width * (16f / 9f)); break;
                case 1: // 4: 3
                    height = (int)(width * (4f / 3f)); break;
                case 2: // 정방
                    height = width; break;
                case 3: // full
                    height = Screen.height; break;
            }
            int PosY = (Screen.height - height) / 2;
            string totalPath = TotalPath; // 프로퍼티 참조 시 시간에 따라 이름이 결정되므로 캐싱
            if (File.Exists(totalPath))
            {
                string temp = totalPath;
                int i = 0;
                do
                {
                    totalPath = temp + "(" + (++i) + ")";

                } while (File.Exists(totalPath));
            }
            Texture2D screenTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            Rect area = new Rect(0f, PosY, width, height);
            Debug.Log(area);
            // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
            screenTex.ReadPixels(area, 0, 0);

            bool succeeded = true;
            try
            {
                // 폴더가 존재하지 않으면 새로 생성
                if (Directory.Exists(FolderPath) == false)
                {
                    Directory.CreateDirectory(FolderPath);
                }

                // 스크린샷 저장
                File.WriteAllBytes(totalPath, screenTex.EncodeToPNG());
            }
            catch (Exception e)
            {
                succeeded = false;
                Debug.LogWarning($"Screen Shot Save Failed : {totalPath}");
                Debug.LogWarning(e);
            }

            // 마무리 작업
            if (succeeded)
            {
                Debug.Log($"Screen Shot Saved : {totalPath}");
                lastSavedPath = totalPath; // 최근 경로에 저장
                ShotImages.Push(totalPath);
                RefreshAndroidGallery(totalPath);
                ReadScreenShotAndShow();
            }

            // 갤러리 갱신

        }
        private void CaptureOnlyExcludeUI()
        {
#if UNITY_ANDROID
            CheckAndroidPermissionAndDo(Permission.ExternalStorageWrite, () => StartCoroutine(CaptureOnlyExcludeUICorutine()));
#else
            StartCoroutine(CaptureOnlyExcludeUICorutine());
#endif
        }
        RenderTexture renderTexture;
        int originalCullingMask;
        IEnumerator CaptureOnlyExcludeUICorutine()
        {
            originalCullingMask = _ARcamera.cullingMask;
            //LayerMask excludedLayer = LayerMask.GetMask("UI");
            _ARcamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
            renderTexture = new RenderTexture(Screen.width, Screen.height, 32);
            _ARcamera.targetTexture = renderTexture;
            _ARcamera.Render();
            yield return new WaitForEndOfFrame();
            CaptureOnlyExcludeUIMain();
        }
        private void CaptureOnlyExcludeUIMain()
        {

            int width, height = 0; // 사진 가로, 세로
            width = Screen.width;

            switch (CameraRatio)
            {
                case 0: // 16:9
                    height = (int)(width * (16f / 9f)); break;
                case 1: // 4: 3
                    height = (int)(width * (4f / 3f)); break;
                case 2: // 정방
                    height = width; break;
                case 3: // full
                    height = Screen.height; break;
            }
            int PosY = (Screen.height - height) / 2;
            string totalPath = TotalPath; // 프로퍼티 참조 시 시간에 따라 이름이 결정되므로 캐싱
            if (File.Exists(totalPath))
            {
                string temp = totalPath;
                int i = 0;
                do
                {
                    totalPath = temp + "(" + (++i) + ")";

                } while (File.Exists(totalPath));
            }

 
            Texture2D screenTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            Rect area = new Rect(0f, PosY, width, height);

            RenderTexture.active = renderTexture;
            Debug.Log(area);
            // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
            screenTex.ReadPixels(area, 0, 0);
            screenTex.Apply();
            bool succeeded = true;
            try
            {
                // 폴더가 존재하지 않으면 새로 생성
                if (Directory.Exists(FolderPath) == false)
                {
                    Directory.CreateDirectory(FolderPath);
                }

                // 스크린샷 저장
                File.WriteAllBytes(totalPath, screenTex.EncodeToPNG());
            }
            catch (Exception e)
            {
                succeeded = false;
                Debug.LogWarning($"Screen Shot Save Failed : {totalPath}");
                Debug.LogWarning(e);
            }
            _ARcamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            _ARcamera.cullingMask = originalCullingMask;
            // 마무리 작업
            if (succeeded)
            {
                Debug.Log($"Screen Shot Saved : {totalPath}");
                lastSavedPath = totalPath; // 최근 경로에 저장
                ShotImages.Push(totalPath);
                RefreshAndroidGallery(totalPath);
                ReadScreenShotAndShow();
            }

            // 갤러리 갱신

        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        private void RefreshAndroidGallery(string imageFilePath)
        {
#if !UNITY_EDITOR
            AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2]
            { "android.intent.action.MEDIA_SCANNER_SCAN_FILE", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + imageFilePath) });
            objActivity.Call("sendBroadcast", objIntent);
#endif
        }

        // 가장 최근에 저장된 이미지 보여주기
        //경로로부터 저장된 스크린샷 파일을 읽어서 이미지에 보여주기
        private void ReadScreenShotFileAndShow(Image destination)
        {
            string folderPath = FolderPath;
            string totalPath = lastSavedPath;


            if (Directory.Exists(folderPath) == false)
            {
                Debug.LogWarning($"{folderPath} 폴더가 존재하지 않습니다.");
                return;
            }
            if (File.Exists(totalPath) == false)
            {
                Debug.LogWarning($"{totalPath} 파일이 존재하지 않습니다.");
                return;
            }
            /*
            // 기존의 텍스쳐 소스 제거
            if (_imageTexture != null)
                Destroy(_imageTexture);
            if (destination.sprite != null)
            {
                destination.sprite = null;
            }
            */
            // 저장된 스크린샷 파일 경로로부터 읽어오기
            try
            {
                byte[] texBuffer = File.ReadAllBytes(totalPath);

                _imageTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                _imageTexture.LoadImage(texBuffer);
            }
            catch (Exception e)
            {

                Debug.LogWarning($"스크린샷 파일을 읽는 데 실패하였습니다.");
                Debug.LogWarning(e);
                return;
            }

            // 이미지 스프라이트에 적용
            Rect rect = new Rect(0, 0, _imageTexture.width, _imageTexture.height);
            Sprite sprite = Sprite.Create(_imageTexture, rect, Vector2.one * 0.5f);
            destination.sprite = sprite;
        }
        public void imageOpen()
        {

            Debug.Log("ShotImages : " + ShotImages);
            GalleryIndex = 0;
            if (!ShotImages.isEmpty())
                ReadScreenShotFileAndShow(galleryToShow, ShotImages.getData(GalleryIndex));
            Gallery.SetActive(true);
        }
        int GalleryIndex;
        public void GalleryNextImage()
        {
            if (GalleryIndex >= ShotImages.Length() - 1) return;
            else
            {
                GalleryIndex++;
                ReadScreenShotFileAndShow(galleryToShow, ShotImages.getData(GalleryIndex));
            }
        }
        public void GalleryPrevImage()
        {
            if (GalleryIndex <= 0) return;
            else
            {
                GalleryIndex--;
                ReadScreenShotFileAndShow(galleryToShow, ShotImages.getData(GalleryIndex));
            }
        }
        public void GalleryExit()
        {
            Gallery.SetActive(false);
        }
        private void ReadScreenShotFileAndShow(Image destination, string path)
        {
            string folderPath = FolderPath;
            string totalPath = path;

            /*
            // 기존의 텍스쳐 소스 제거
            if (_galleryTexture != null)
                Destroy(_galleryTexture);
            if (destination.sprite != null)
            {
                destination.sprite = null;
            }
            */
            // 저장된 스크린샷 파일 경로로부터 읽어오기
            try
            {
                byte[] texBuffer = File.ReadAllBytes(totalPath);

                _galleryTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                _galleryTexture.LoadImage(texBuffer);
            }
            catch (Exception e)
            {

                Debug.LogWarning($"스크린샷 파일을 읽는 데 실패하였습니다.");
                Debug.LogWarning(e);
                return;
            }

            // 이미지 스프라이트에 적용
            Rect rect = new Rect(0, 0, _galleryTexture.width, _galleryTexture.height);
            Sprite sprite = Sprite.Create(_galleryTexture, rect, Vector2.one * 0.5f);
            destination.sprite = sprite;
            destination.SetNativeSize();
        }
        public void ShareImage()
        {
            if (ShotImages.isEmpty()) return;
            string path = ShotImages.getData(GalleryIndex);
            if (File.Exists(path))
            {
                new NativeShare().AddFile(path).Share();
            }
        }
        public void DeleteImage()
        {
            if (ShotImages.isEmpty()) return;
            string path = ShotImages.getData(GalleryIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
                ShotImages.Remove(path);
                if (ShotImages.isEmpty())
                {
                    setDefaultImagetoGallery();
                    GalleryIndex = 0;
                }


                else
                {

                    if (GalleryIndex >= ShotImages.Length())
                    {
                        GalleryIndex = ShotImages.Length() - 1;
                    }
                    ReadScreenShotFileAndShow(galleryToShow, ShotImages.getData(GalleryIndex));

                }




            }
        }
        private void setDefaultImagetoGallery()
        {
            galleryToShow.sprite = null;
            imageToShow.sprite = null;
        }
        public void openMenu()
        {
            GameObject button = EventSystem.current.currentSelectedGameObject;
            if (!Menu.activeSelf)
            {
                Menu.SetActive(true);
                button.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                Menu.SetActive(false);
                button.transform.localEulerAngles = new Vector3(0, 0, 90);

            }
        }
        public void KUScale(Slider slider)
        {
            GameObject ku = GameObject.FindWithTag("KU");
            if (ku.name.Equals("KU_QR1"))
            {
                ku.transform.localScale = new Vector3(slider.value, slider.value, slider.value);
            }
            else if (ku.name.Equals("KU_QR2"))
            {
                ku.transform.localScale = new Vector3(slider.value / 5f, slider.value / 5f, slider.value / 5f);
            }

        }
        public void isWithUI(Toggle toggle)
        {
            if (toggle.isOn)
            {
                //screenShotWithoutUIButton.onClick.RemoveListener(TakeScreenShotWithoutUI);
                screenShotWithoutUIButton.onClick.RemoveListener(CaptureOnlyExcludeUI);
                screenShotWithoutUIButton.onClick.RemoveListener(StartFlash);
                screenShotWithoutUIButton.onClick.AddListener(TakeScreenShotFull);
            }
            else
            {
                screenShotWithoutUIButton.onClick.RemoveListener(TakeScreenShotFull);
                screenShotWithoutUIButton.onClick.AddListener(StartFlash);
                //screenShotWithoutUIButton.onClick.AddListener(TakeScreenShotWithoutUI);
                screenShotWithoutUIButton.onClick.AddListener(CaptureOnlyExcludeUI);
            }
        }

        public void changeLight(Slider slider)
        {
        }
        IEnumerator FlashEffect()
        {
            flashObject.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            flashObject.SetActive(false);
        }


        public void StartFlash()
        {
            StartCoroutine("FlashEffect");

        }
        public void FilterSet(int i)
        {
            if (i == 2) Filter.SetActive(false);
            else
            {
                FilterImgs[0].sprite = FilterSprites[i * 2];
                FilterImgs[1].sprite = FilterSprites[i * 2 + 1];
                Filter.SetActive(true);
            }
        }

        #endregion
    }
}