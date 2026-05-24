using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MultiObjectAugmenter : MonoBehaviour
{
    
    //can put any number of images
    [System.Serializable]
    public struct ObjectPrefabPair
    {
        public string objectName;
        public GameObject prefab;
    }

    [Header("Object Mapping Configuration")]
    public List<ObjectPrefabPair> objectPrefabPairs;

    private ARTrackedObjectManager _objectManager;
    
    //a dictionary to keep track of spawned objects
    private Dictionary<string, GameObject> _instances = new Dictionary<string, GameObject>();
    //same as part of the script from the lecture
    void Awake()
    {
        _objectManager = GetComponent<ARTrackedObjectManager>();
    }

    //same as part of the script from the lecture
    void OnEnable()
    {
        if (_objectManager != null)
            _objectManager.trackablesChanged.AddListener(OnObjectsChanged);
    }

    //same as part of the script from the lecture
    void OnDisable()
    {
        if (_objectManager != null)
            _objectManager.trackablesChanged.RemoveListener(OnObjectsChanged);
    }

    private void OnObjectsChanged(ARTrackablesChangedEventArgs<ARTrackedObject> args)
    {
        //1 - when an object is detected for the first time
        foreach (var obj in args.added)
        {
            string objectName = obj.referenceObject.name;
            GameObject selectedPrefab = null;

            //loop through the list
            foreach (var pair in objectPrefabPairs)
            {
                if (pair.objectName == objectName)
                {
                    selectedPrefab = pair.prefab;
                    break;
                }
            }

            //spawn it if we found a match and haven't spawned it yet
            if (selectedPrefab != null && !_instances.ContainsKey(objectName))
            {
                var newInstance = Instantiate(selectedPrefab, obj.transform);
                _instances.Add(objectName, newInstance);
            }
        }

        //handle visibility (hides the prefab if tracking is lost)
        foreach (var obj in args.updated)
        {
            if (_instances.ContainsKey(obj.referenceObject.name))
            {
                _instances[obj.referenceObject.name].SetActive(
                    obj.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking
                );
            }
        }
    }
}