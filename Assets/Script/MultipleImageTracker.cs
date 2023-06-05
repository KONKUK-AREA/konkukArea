using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MultipleImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    [SerializeField] // private 이어도 유니티 인스펙터 창에 변수가 뜨게 해 줌
    private GameObject[] placeablePrefabs;


    private Dictionary<string, GameObject> spanwedObjects;

    private void Awake () 
    {
      trackedImageManager = GetComponent<ARTrackedImageManager>();
      spanwedObjects= new Dictionary<string, GameObject>();

      foreach (GameObject obj in placeablePrefabs)
      {
        GameObject newObject = Instantiate(obj);
        newObject.name = obj.name;
        newObject.SetActive(false);

        spanwedObjects.Add(newObject.name, newObject);

      }

    }
        void UpdateSpawnObject(ARTrackedImage trackedImage)
    {
        string referenceImageName = trackedImage.referenceImage.name;   // 레퍼런스 이미지와 불러올 오브젝트의 string이름이 같아야 불러오게 됨
        spanwedObjects[referenceImageName].transform.position = trackedImage.transform.position;        
        spanwedObjects[referenceImageName].transform.rotation = trackedImage.transform.rotation;

        spanwedObjects[referenceImageName].SetActive(true); 


    }



  


    void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach(ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateSpawnObject(trackedImage);

        }
        foreach(ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateSpawnObject(trackedImage);
        }
        foreach(ARTrackedImage trackedImage in eventArgs.removed)
        {
            spanwedObjects[trackedImage.name].SetActive(false); 
        }

    }

     private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImageChanged;

    }

    private void OnDisable() 
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
        
    }





    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"There are {trackedImageManager.trackables.count} images being tracked");    // 몇개의 이미지 인식되었는지 확인하는 로그

        foreach( var trackedImage in trackedImageManager.trackables)  // 어느위치에 있는지 확인하기 위한 로그
        {
            Debug.Log($"Image: {trackedImage.referenceImage.name} is at " + $"{trackedImage.transform.position}");
                  
       }

    }
}
