using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CameraAlignedTextureProjection : MonoBehaviour
{
    public Texture inputTexture; // Assign the texture in the Inspector
    public Camera projectorCamera; // Assign the camera used for the projection

    private Material material;

    void Start()
    {
        // Get the material of the object this script is attached to
        material = GetComponent<Renderer>().material;

        // Assign the texture to the shader
        if (inputTexture != null)
            material.SetTexture("_MainTex", inputTexture);
    }

    void Update()
    {
        if (projectorCamera != null)
        {
            // Get the view-projection matrix
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(projectorCamera.projectionMatrix, false);
            Matrix4x4 viewMatrix = projectorCamera.worldToCameraMatrix;

            // Combine view and projection matrices
            Matrix4x4 viewProjectionMatrix = projectionMatrix * viewMatrix;

            // Pass the matrix to the shader
            material.SetMatrix("_ViewProjection", viewProjectionMatrix);
        }
    }
}
