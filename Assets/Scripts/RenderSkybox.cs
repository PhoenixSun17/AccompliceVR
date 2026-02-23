using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RenderSkybox : MonoBehaviour
{
    public RenderTexture sourceRenderTexture; // Assign the Remote RenderTexture 
    public int cubemapResolution = 1024;      // Resolution for the cubemap faces
    private Camera captureCamera;             // The camera used to render each face
    public Cubemap cubemap;
    public Material skyboxMat;
    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 45;
    }

    void Update()
    {
        // Create a new cubemap with the desired resolution
        // cubemap = new Cubemap(cubemapResolution, TextureFormat.RGB24, false);

        // Set up a new camera if one isn¡¯t assigned
        if (captureCamera == null)
        {
            Debug.Log("Create new Camera");
            GameObject cameraObject = new GameObject("CubemapCaptureCamera");
            captureCamera = cameraObject.AddComponent<Camera>();
            captureCamera.enabled = false; // Disable to avoid rendering to screen
        }
        // Copy RenderTexture content to each face of the cubemap
        CopyRenderTextureToCubemap();
        
    }

    //private void LateUpdate()
    //{
    //    Destroy(cubemap);
    //}

    void CopyRenderTextureToCubemap()
    {
        if (sourceRenderTexture == null)
        {
            Debug.LogError("Source RenderTexture is not assigned!");
            return;
        }
        
        // Set the camera to use the source RenderTexture
        captureCamera.targetTexture = sourceRenderTexture;
        captureCamera.Render();
        Texture2D tex2d = new Texture2D(1024,1024);
        tex2d.ReadPixels(captureCamera.rect,0,0,true);
        // Render into the cubemap
        // Debug.Log(cubemap);
        Color[] pixels = tex2d.GetPixels();
        //bool what = captureCamera.RenderToCubemap(cubemap);
        cubemap.SetPixels(pixels, CubemapFace.PositiveX); 
        //Debug.Log(what);

        skyboxMat.SetTexture("_Tex", cubemap);


        // Use the cubemap, for example, as a skybox or reflection texture
        RenderSettings.skybox = skyboxMat;
        Debug.Log("RenderTexture copied to Cubemap successfully.");
    }
}