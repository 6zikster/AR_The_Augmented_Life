using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MultiImageAugmenter : MonoBehaviour
{
    //can put any number of images
    [System.Serializable]
    public struct ImagePrefabPair
    {
        public string imageName;
        public GameObject prefab;
    }

    [Header("Image Mapping Configuration")]
    public List<ImagePrefabPair> imagePrefabPairs;

    private ARTrackedImageManager _imageManager;
    
    //a dictionary to keep track of spawned objects
    private Dictionary<string, GameObject> _instances = new Dictionary<string, GameObject>();


    //same as part of the script from the lecture
    void Awake()
    {
        _imageManager = GetComponent<ARTrackedImageManager>();
    }
    //same as part of the script from the lecture
    void OnEnable()
    {
        if (_imageManager != null)
            _imageManager.trackablesChanged.AddListener(OnImagesChanged);
    }
    //same as part of the script from the lecture
    void OnDisable()
    {
        if (_imageManager != null)
            _imageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }


    private void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        //1 - when an image is detected for the first time
        foreach (var img in args.added)
        {
            //ignore img if it's not in the references
            if (img.referenceImage.guid == System.Guid.Empty) continue;

            string imageName = img.referenceImage.name;
            GameObject selectedPrefab = null;

            //loop through the list
            foreach (var pair in imagePrefabPairs)
            {
                if (pair.imageName == imageName)
                {
                    selectedPrefab = pair.prefab;
                    break; //found it
                }
            }

            //spawn it if we found a prefab and haven't spawned it yet
            if (selectedPrefab != null && !_instances.ContainsKey(imageName))
            {
                var newInstance = Instantiate(selectedPrefab, img.transform);
                _instances.Add(imageName, newInstance);
            }
        }

        // handle visibility
        foreach (var img in args.updated)
        {
            //ignore img if it's not in the references
            if (img.referenceImage.guid == System.Guid.Empty) continue;
            if (_instances.ContainsKey(img.referenceImage.name))
            {
                _instances[img.referenceImage.name].SetActive(
                    img.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking
                );
            }
        }
    }
}