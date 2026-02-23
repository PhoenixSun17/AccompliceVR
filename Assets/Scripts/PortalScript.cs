using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Ubiq.Avatars;
//using UnityEditor.Experimental.GraphView;

public class PortalScript: MonoBehaviour
{
    public Camera targetCamera;
    public GameObject targetRoot;
    public float portalDistance=2f;
    public float maxApparentSize = 5f; // Maximum apparent size for the portal
    public float minPortalScale = 0.1f;  // Minimum scale for the portal

    private CVRSystem vrSystem;  // The SteamVR system interface
    private CVRInput vrInput;
    private uint poseDeviceIndex;  // The device index of the headset

    // Start is called before the first frame update

    void Start()
    {
                // Initialize OpenVR
        var initError = EVRInitError.None;
        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Overlay);
//        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Other);

        if (initError != EVRInitError.None)
        {
            Debug.LogError("Failed to initialize OpenVR: " + initError);
            return;
        }

        // Get the SteamVR system interface
        vrSystem = OpenVR.System;
        vrInput = OpenVR.Input;

        // Get the device index of the headset
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            ETrackedDeviceClass deviceClass = vrSystem.GetTrackedDeviceClass(i);
            if (deviceClass == ETrackedDeviceClass.HMD)
            {
                poseDeviceIndex = i;
                break;
            }
        }
    }

void PositionAndScalePortal(Transform portal)
{
    // Step 1: Calculate the Oriented Bounding Box (OBB) for the targetRoot
    Bounds combinedBounds = CalculateCombinedBoundsRecursive(targetRoot.transform);

    // Get world space AABB (axis-aligned bounding box)
    Vector3[] worldCorners = GetWorldSpaceCorners(combinedBounds, targetRoot.transform);

    // Step 2: Calculate the distance to the object and portal
    Vector3 cameraPos = targetCamera.transform.position;
    Vector3 objectCenter = combinedBounds.center;

    float distanceToObject = Vector3.Distance(cameraPos, objectCenter);
    Vector3 portalPosition;

    // Set the portal at the midpoint or portalDistance away, whichever is farther
    if (distanceToObject < portalDistance)
    {
        Vector3 directionToObject = (objectCenter - cameraPos).normalized;
        portalPosition = cameraPos + directionToObject * portalDistance;
    }
    else
    {
        portalPosition = (cameraPos + objectCenter) / 2f;
    }

    // Set portal position before calculating the projected size
    portal.position = portalPosition;

    // Rotate the portal to face the camera
    Vector3 directionToCamera = (portalPosition - cameraPos).normalized;
    Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera);
    portal.rotation = rotationToCamera;

    // Step 3: Project the world corners onto the portal plane
    Vector3 portalRight = portal.right;  // Local right direction of the portal
    Vector3 portalUp = portal.up;        // Local up direction of the portal

    // Calculate the projected size of the object on the portal
    Vector2 projectedSize = GetProjectedSize(worldCorners, portal, targetCamera, portalRight, portalUp);

    // Step 4: Scale the portal based on the projected size
    float portalScale = Mathf.Max(projectedSize.x, projectedSize.y);

    // Enforce minimum and maximum scale limits
    portalScale = Mathf.Clamp(portalScale, minPortalScale, maxApparentSize);

    // Apply the calculated scale to the portal
    portal.localScale = new Vector3(portalScale, portalScale, 1f);
}



    Vector2 GetProjectedSize(Vector3[] worldCorners, Transform portal, Camera camera, Vector3 portalRight, Vector3 portalUp)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        // Calculate the distance from the camera to the portal
        float distanceToPortal = Vector3.Distance(camera.transform.position, portal.position);

        foreach (Vector3 corner in worldCorners)
        {
            //// Calculate the distance from the camera to the current corner
            //float distanceToCorner = Vector3.Distance(camera.transform.position, corner);

            // Get the offset from the portal position
            Vector3 offset = corner - portal.position;

            // Project onto the portal's local right and up directions
            float x = Vector3.Dot(offset, portalRight);
            float y = Vector3.Dot(offset, portalUp);

            // Adjust the size of the projection based on the ratio of distances
            //float scaleFactor = distanceToPortal / distanceToCorner;
            //x *= scaleFactor;
            //y *= scaleFactor;

            // Track min/max extents in the portal's local plane
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        // Return the size in the portal's local space
        return new Vector2(maxX - minX, maxY - minY);
    }

    // Get world-space AABB corners of the combined bounds
    Vector3[] GetWorldSpaceCorners(Bounds bounds, Transform root)
    {
        Vector3[] corners = new Vector3[8];

        // Bounds extents in local space
        Vector3 extents = bounds.extents;

        // Generate the 8 corners of the AABB in local space
        corners[0] = bounds.center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = bounds.center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[2] = bounds.center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = bounds.center + new Vector3(extents.x, extents.y, -extents.z);
        corners[4] = bounds.center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[5] = bounds.center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = bounds.center + new Vector3(-extents.x, extents.y, extents.z);
        corners[7] = bounds.center + new Vector3(extents.x, extents.y, extents.z);

        // Transform the corners from local space to world space
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = root.TransformPoint(corners[i]);
        }

        return corners;
    }

    // Recursive method to calculate the combined bounding box for the root transform and its children
    Bounds CalculateCombinedBoundsRecursive(Transform root)
    {
        Renderer rootRenderer = root.GetComponent<Renderer>();
        Bounds combinedBounds;

        if (rootRenderer != null)
        {
            combinedBounds = rootRenderer.bounds;
        }
        else
        {
            combinedBounds = new Bounds(root.position, Vector3.zero);
        }

        foreach (Transform child in root)
        {
            Bounds childBounds = CalculateCombinedBoundsRecursive(child);
            combinedBounds.Encapsulate(childBounds);
        }

        return combinedBounds;
    }

    void AdjustCameraToPortal(Camera camera, Transform portal)
    {
        camera.aspect = 1.0f;

        float portalSize = portal.localScale.x;
        float distanceToPortal = Vector3.Distance(camera.transform.position, portal.position);

        float halfPortalSize = portalSize / 2.0f;
        float fovRadians = 2 * Mathf.Atan(halfPortalSize / distanceToPortal);
        float fovDegrees = fovRadians * Mathf.Rad2Deg;
        //Debug.Log("FOV"+fovDegrees);
        //Debug.Log("Portal size" + portalSize);

        camera.fieldOfView = fovDegrees;

        Vector3 directionToPortal = (portal.position - camera.transform.position).normalized;
        Quaternion rotationToPortal = Quaternion.LookRotation(directionToPortal);
        camera.transform.rotation = rotationToPortal;
    }


    // Update is called once per frame
    void Update()
    {
        if (targetRoot == null) return;

        // Get the position and rotation of the headset
        TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);
        SteamVR_Utils.RigidTransform trans = new SteamVR_Utils.RigidTransform(poses[poseDeviceIndex].mDeviceToAbsoluteTracking);
        targetCamera.transform.localPosition = trans.pos;

#if false
        // Set the position and rotation of the object


        Vector3 A = targetCamera.transform.position; 
        A.y = 0;
        Vector3 B = targetDummy.transform.position;
        B.y = 0;
        Vector3 midpoint = (A + B ) / 2f;
        Vector3 direction = (midpoint - A).normalized;
        Vector3 C = A + direction * portalDistance;
        transform.position = C;
        Vector3 directionToA = (C  - A).normalized;
        Quaternion rotationToA = Quaternion.LookRotation(directionToA);
        transform.rotation = rotationToA;

        targetCamera.transform.rotation = rotationToA;
#else

        PositionAndScalePortal(transform);
        AdjustCameraToPortal(targetCamera.GetComponent<Camera>(), transform);

#endif
    }
}
