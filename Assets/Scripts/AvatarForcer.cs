using UnityEngine;
using Valve.VR;
using Ubiq.Avatars;

public class AvatarForcer : MonoBehaviour
{

    public GameObject targetAvatars;
    public GameObject targetAvatar;
    public GameObject head;
    public GameObject lhand;
    public GameObject rhand;
    public FrameAlignment frameAlignment;
    private int frameCount = 0;
    private CVRSystem vrSystem;  // The SteamVR system interface
    private uint hmdIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint leftControllerIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint rightControllerIndex = OpenVR.k_unTrackedDeviceIndexInvalid;


        // Separate method to find device indices
    private void FindDeviceIndices()
    {
        // Guard clause: Return if all devices have already been found
        if (hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
            leftControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
            rightControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            return;
        }

        // Get the device index of the headset, left, and right controllers
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            ETrackedDeviceClass deviceClass = vrSystem.GetTrackedDeviceClass(i);

            // Find the HMD
            if (deviceClass == ETrackedDeviceClass.HMD && hmdIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                hmdIndex = i;
                Debug.Log("HMD found at index: " + hmdIndex);
            }

            // Find controllers (left and right)
            if (deviceClass == ETrackedDeviceClass.Controller)
            {
                ETrackedControllerRole role = vrSystem.GetControllerRoleForTrackedDeviceIndex(i);

                if (role == ETrackedControllerRole.LeftHand && leftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    leftControllerIndex = i;
                    Debug.Log("Left controller found at index: " + leftControllerIndex);
                }

                if (role == ETrackedControllerRole.RightHand && rightControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    rightControllerIndex = i;
                    Debug.Log("Right controller found at index: " + rightControllerIndex);
                }
            }

            // Early exit if all devices are found
            if (hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
                leftControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
                rightControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                break;
            }
        }

        // Handle case where devices are not found
        if (hmdIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            Debug.LogWarning("HMD not found");
        }
        if (leftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            Debug.LogWarning("Left controller not found");
        }
        if (rightControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            Debug.LogWarning("Right controller not found");
        }
    }

    void Start()
    {
                // Initialize OpenVR
        var initError = EVRInitError.None;
        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Other);

        if (initError != EVRInitError.None)
        {
            Debug.LogError("Failed to initialize OpenVR: " + initError);
            return;
        }

        vrSystem = OpenVR.System;

    }

        // Helper function to recursively search for a child object by name
    private GameObject FindChildByName(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name.ToLower() == name.ToLower())  // Match the name (case-insensitive)
            {
                return child.gameObject;
            }

            // Recursively search in this child's children
            GameObject found = FindChildByName(child.gameObject, name);
            if (found != null)
            {
                return found;
            }
        }

        // Return null if the object with the specified name is not found
        return null;
    }

    public void SendPosAndNum()
    {
        if (head != null)
        {
            frameAlignment.SendMessage(frameCount, head.transform.position, head.transform.rotation);
            frameCount += 1;
            Debug.Log("Send Frame alignment message");
        }

    }

    void Update()
    {

        FindDeviceIndices();

        // Check if targetObject is assigned
        if (targetAvatar == null && targetAvatars != null)
        {

            //Select target
            if (targetAvatars.transform.childCount > 0)
            {
                foreach (Transform child in targetAvatars.transform)
                {
                    Ubiq.Avatars.Avatar avatar = child.gameObject.GetComponent<Ubiq.Avatars.Avatar>();
                    if (avatar != null && avatar.IsLocal)
                    {
                        targetAvatar = child.gameObject;
                        //head = FindChildByName(targetAvatar, "Floating_Head");
                        //lhand = FindChildByName(targetAvatar, "Floating_LeftHand_A");
                        //rhand = FindChildByName(targetAvatar, "Floating_RightHand_A");
                    }
                }
            }
        }


        if (targetAvatar != null)
        {
            if (head != null)
            {
                // Get the position and rotation of the headset
                TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

                if (hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    SteamVR_Utils.RigidTransform trans = new SteamVR_Utils.RigidTransform(poses[hmdIndex].mDeviceToAbsoluteTracking);
                    head.transform.position = trans.pos;
                    head.transform.rotation = trans.rot;
                }
                if (leftControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    SteamVR_Utils.RigidTransform trans = new SteamVR_Utils.RigidTransform(poses[leftControllerIndex].mDeviceToAbsoluteTracking);
                    lhand.transform.position = trans.pos;
                    lhand.transform.rotation = trans.rot;
                }

                if (rightControllerIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    SteamVR_Utils.RigidTransform trans = new SteamVR_Utils.RigidTransform(poses[rightControllerIndex].mDeviceToAbsoluteTracking);
                    rhand.transform.position = trans.pos;
                    rhand.transform.rotation = trans.rot;
                }
            }
        }
    }
}
