using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class AnchoredOverlay : MonoBehaviour
{
    //    public Texture2D checkerboardTexture;
    public Camera targetCamera;
    public GameObject targetPortal;

    private CVROverlay overlay;
    public ulong overlayHandle;
    public Vector3 offset;
    private bool openVRInitialised = false;

    public HmdMatrix34_t hmdMatrix;

    public Matrix4x4 rotatedMatrix;

    private bool InitOpenVR()
    {
        // Init OpenVR
        EVRInitError error = EVRInitError.None;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

        if (error == EVRInitError.None)
        {
            return true;
        }
        else
        {
            Debug.LogError("Failed to initialise OpenVR: " + error.ToString());
            return false;
        }
    }

    void Start()
    {

        if (!openVRInitialised)
        {
            openVRInitialised = InitOpenVR();
        }


        // Get the OpenVR overlay interface
        overlay = OpenVR.Overlay;

        // Create the overlay and get its handle
        overlay.CreateOverlay("Cloner", "Cloner Overlay", ref overlayHandle);

        // Set the overlay texture to the checkerboard pattern
        var texture = new Texture_t();
        texture.handle = targetCamera.targetTexture.GetNativeTexturePtr();
        texture.eType = ETextureType.DirectX;
        texture.eColorSpace = EColorSpace.Gamma;

        overlay.SetOverlayTexture(overlayHandle, ref texture);
        VRTextureBounds_t bounds = new VRTextureBounds_t();
        bounds.uMin = 0f;
        bounds.uMax = 1f;
        bounds.vMin = 1f;
        bounds.vMax = 0f;

        overlay.SetOverlayTextureBounds(overlayHandle, ref bounds);

        // Set the overlay size and position
        overlay.SetOverlayWidthInMeters(overlayHandle, 1.0f);

        overlay.ShowOverlay(overlayHandle);
    }

    void UpdateOverlayTexture()
    {
        if (overlay == null) return;
        // Capture a new frame from the camera and update the overlay texture
        targetCamera.Render();
        var texture = new Texture_t();
        texture.handle = targetCamera.targetTexture.GetNativeTexturePtr();
        texture.eType = ETextureType.DirectX;
        texture.eColorSpace = EColorSpace.Gamma;
        overlay.SetOverlayTexture(overlayHandle, ref texture);

        // Convert transform

        // Extract the position, rotation, and scale from the Unity Transform
        Vector3 position = targetPortal.transform.position;
        Quaternion rotation = targetPortal.transform.rotation;
        Vector3 scale = targetPortal.transform.localScale; // Get the scale

        // Create a rotation of 180 degrees around the transform's local Y-axis
        Quaternion rotationY180 = Quaternion.Euler(0, 180, 0); // Local rotation
        Quaternion localRotation = rotation; // Combine with original rotation
                                             //        Quaternion localRotation = rotationY180 * rotation; // Combine with original rotation
                                             //        Quaternion localRotation = rotation * rotationY180; // Combine with original rotation

        // Create a new Matrix4x4 with the updated rotation
        rotatedMatrix = Matrix4x4.TRS(position, localRotation, scale);

        // Define a flip matrix to swap Z-axis
        Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1)); // Correctly flips Z

        // Apply the coordinate transformation
        rotatedMatrix = flipZ * rotatedMatrix * flipZ;

        // Convert to HmdMatrix34_t (3x4 matrix)
        hmdMatrix = new HmdMatrix34_t();
        //        HmdMatrix34_t hmdMatrix = new HmdMatrix34_t();

        hmdMatrix.m0 = rotatedMatrix.m00;
        hmdMatrix.m1 = rotatedMatrix.m01;
        hmdMatrix.m2 = rotatedMatrix.m02;
        hmdMatrix.m3 = rotatedMatrix.m03; // X position

        hmdMatrix.m4 = rotatedMatrix.m10;
        hmdMatrix.m5 = rotatedMatrix.m11;
        hmdMatrix.m6 = rotatedMatrix.m12;
        hmdMatrix.m7 = rotatedMatrix.m13; // Y position

        hmdMatrix.m8 = rotatedMatrix.m20;
        hmdMatrix.m9 = rotatedMatrix.m21;
        hmdMatrix.m10 = rotatedMatrix.m22;
        hmdMatrix.m11 = rotatedMatrix.m23; // Z position


        overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref hmdMatrix);
        //        overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, ref openVRMatrix);

    }


    private void Update()
    {
        UpdateOverlayTexture();
    }

    private void OnApplicationQuit()
    {
        //if (openVRInitialised) {
        //    overlay.DestroyOverlay(overlayHandle);
        //    overlay = null;
        //    OpenVR.Shutdown();
        //    openVRInitialised = false;
        //}
    }

}