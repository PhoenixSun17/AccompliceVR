using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CameraTracker : MonoBehaviour
{
    private CVRSystem vrSystem;  // The SteamVR system interface
    private CVRInput vrInput;
    private uint poseDeviceIndex;  // The device index of the headset

    // Start is called before the first frame update
    void Start()
    {
        // Initialize OpenVR
        var initError = EVRInitError.None;
//        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Overlay);
        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Other);

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

    // Update is called once per frame
    void Update()
    {
        // Get the position and rotation of the headset
        TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);
        SteamVR_Utils.RigidTransform trans = new SteamVR_Utils.RigidTransform(poses[poseDeviceIndex].mDeviceToAbsoluteTracking);

        // Set the position and rotation of the object
        transform.localPosition = trans.pos;
        transform.localRotation = trans.rot;
    }
}
