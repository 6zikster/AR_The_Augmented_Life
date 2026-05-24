using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;


[RequireComponent(typeof(ARPlaneManager))]
public class PlaneTypeDetector : MonoBehaviour
{
    private ARPlaneManager _planeManager;
    
    public GameObject customCubePrefab;
    private bool isDetectionEnabled = false;
    void Awake()
    {
        _planeManager = GetComponent<ARPlaneManager>();

        if (_planeManager != null)
        {
            _planeManager.enabled = false;
        }
    }

    void OnEnable()
    {
        if (_planeManager != null)
        {
            // In AR Foundation 6+, trackablesChanged is a UnityEvent
            // Use AddListener instead of +=
            _planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        }
    }

    void OnDisable()
    {
        if (_planeManager != null)
        {
            // Use RemoveListener instead of -=
            _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        if (!isDetectionEnabled) return;


        //this function is being called each time when the plane is changing
        foreach (var plane in args.added)
        {
            UpdatePlaneColor(plane);
        }
        
        foreach (var plane in args.updated)
        {
            UpdatePlaneScale(plane);
        }

        foreach (var kvp in args.removed)
        {
            HandlePlaneRemoved(kvp.Value);
        }
    }

    void UpdatePlaneColor(ARPlane plane)
    {
        if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            if (plane.transform.childCount > 0) return;

            if (customCubePrefab != null)
            {
                GameObject spawnedCube = Instantiate(customCubePrefab, plane.transform.position, plane.transform.rotation);
                spawnedCube.transform.SetParent(plane.transform);
                UpdatePlaneScale(plane);
            }
        }
    }
    
    void HandlePlaneRemoved(ARPlane plane)
    {
        // When AR Foundation merges or deletes a plane, destroy any cubes attached to it
        foreach (Transform child in plane.transform)
        {
            Destroy(child.gameObject);
        }
    }


    void UpdatePlaneScale(ARPlane plane)
    {
        if (plane.alignment != PlaneAlignment.HorizontalUp) return;
        if (plane.transform.childCount == 0) return;

        // Get the nested cube
        Transform cubeTransform = plane.transform.GetChild(0);

        if (cubeTransform != null)
        {
            // AR Plane size.x is width, size.y is length. 
            // We preserve the cube's local Y scale so it keeps its original thickness.
            cubeTransform.localScale = new Vector3(plane.size.x, cubeTransform.localScale.y, plane.size.y);
        }
    }


    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            isDetectionEnabled = !isDetectionEnabled; 
            _planeManager.enabled = isDetectionEnabled; 
            Debug.Log($"T is pressed. Starting detecting horizontal planes.");
        }
    }
}