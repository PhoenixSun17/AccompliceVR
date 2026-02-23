using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System.IO;
using UnityEngine.Rendering;
using System;
using Unity.RenderStreaming;
using System.Linq;

public class Streamer : MonoBehaviour
{
    public Camera viewpoint;
    public Image DisplayImage;
    public VideoStreamSender videoStreamer;
    public AvatarForcer avatarForcer;
    public RenderTexture targetTexture; // Unused now, kept for legacy/comments.
    public ComputeShader computeShader;
    public Transform syncTransform;

    private bool openVRInitialised = false;
    private bool textureCaptured = false;

    System.IntPtr nativeTex = System.IntPtr.Zero;
    public int width = 0;
    public int height = 0;
    static Texture2D mirrorTexture;

    // Start is called before the first frame update
    void Start()
    {
        // targetTexture.enableRandomWrite = true; // Not used anymore.
        Application.targetFrameRate = 60;
        targetTexture.enableRandomWrite = true;

        targetTexture.Create();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quitting Application");
        if (textureCaptured)
        {
            Debug.Log("Q: Destroying Unity Texture Object");
            Destroy(mirrorTexture);
            textureCaptured = false;

            // Debug.Log("Q: Release Texture in OpenVR");
            // OpenVR.Compositor.ReleaseMirrorTextureD3D11(nativeTex);

            Debug.Log("Q: Shut down OpenVR");
            OpenVR.Shutdown();

            openVRInitialised = false;
        }
    }

    private void OnEnable()
    {
        if (!textureCaptured)
        {
            textureCaptured = CaptureTexture();
        }
    }

    private void OnDisable()
    {
        if (textureCaptured)
        {
            Debug.Log("D: Destroying Unity Texture Object");
            Destroy(mirrorTexture);
            textureCaptured = false;

            Debug.Log("D: Release Texture in OpenVR");
            OpenVR.Compositor.ReleaseMirrorTextureD3D11(nativeTex);

            Debug.Log("D: Shut down OpenVR");
            OpenVR.Shutdown();

            openVRInitialised = false;
        }
    }

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

    private bool CaptureTexture()
    {
        // if openvr is not initialised, try initialising it.
        if (!openVRInitialised)
        {
            openVRInitialised = InitOpenVR();
        }

        // Check whether it is initialised now
        if (openVRInitialised)
        {
            // Retrieve texture
            var tex = new Texture2D(2, 2);
            nativeTex = System.IntPtr.Zero;
            if (OpenVR.Compositor.GetMirrorTextureD3D11(EVREye.Eye_Right, tex.GetNativeTexturePtr(), ref nativeTex) == EVRCompositorError.None)
            {
                uint uWidth = 0, uHeight = 0;
                OpenVR.System.GetRecommendedRenderTargetSize(ref uWidth, ref uHeight);
                //Debug.Log(uWidth + "x" + uHeight);  
                // save retrieved values
                width = 3096;//(int)uWidth;
                height = 3408; // (int)uHeight;

                mirrorTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, nativeTex);
                //Debug.Log(mirrorTexture.format);
                //Texture2D tempTexture = mirrorTexture;
                //tempTexture = EmbedMetadata(tempTexture);

                videoStreamer.source = VideoStreamSource.Texture;
                videoStreamer.sourceTexture = targetTexture;

                if (DisplayImage != null)
                {
                    // set texture
                    DisplayImage.material.mainTexture = targetTexture;

                    // set aspect ratio
                    AspectRatioFitter arFitter = DisplayImage.gameObject.GetComponent<AspectRatioFitter>();
                    if (arFitter != null)
                    {
                        arFitter.aspectRatio = (float)width / (float)height;
                    }
                }
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!textureCaptured)
        {
            textureCaptured = CaptureTexture();
        }

        // Copy the mirror texture (read-only) into targetTexture (writable)
        Graphics.Blit(mirrorTexture, targetTexture);

        // Write pose data into targetTexture

        // ModifyPixel();


        // Request GPU readback from targetTexture
        // AsyncGPUReadback.Request(targetTexture, 0, TextureFormat.RGBA32, OnDataRead);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AsyncGPUReadback.Request(targetTexture, 0, TextureFormat.RGBA32, OnCompleteReadback);
        }
    }

    private Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        // Save current active RenderTexture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        // Read pixels into Texture2D
        Texture2D tempTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tempTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tempTexture.Apply();

        // Restore active RenderTexture
        RenderTexture.active = previous;
        return tempTexture;
    }

    private void SetPixelCPU()
    {
        Texture2D tempTexture = RenderTextureToTexture2D(targetTexture);
        Color[] whiteColors = Enumerable.Repeat(Color.white, 10000).ToArray();

        // Modify the pixel
        tempTexture.SetPixels(0, 0, 100, 100, whiteColors);
        tempTexture.Apply(); // Apply changes

        // Write back to RenderTexture
        Graphics.Blit(tempTexture, targetTexture);
    }



    private void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        int width = mirrorTexture.width;
        int height = mirrorTexture.height;

        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture2D.LoadRawTextureData(request.GetData<byte>());
        texture2D.Apply();
        Debug.Log("Fetched " + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));

        //byte[] pngData = texture2D.EncodeToPNG();

        //string fileName = "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png";
        //string filePath = Path.Combine(Application.dataPath, fileName);
        //File.WriteAllBytes(filePath, pngData);

        AsyncGPUReadback.Request(mirrorTexture, 0, TextureFormat.RGBA32, OnCompleteReadback);
    }

    private void OnDataRead(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("GPU readback error!");
            return;
        }

        var requestData = request.GetData<Color32>();
        int width = targetTexture.width;
        int startX = 100;
        int startY = 100;
        int blockWidth = 5;
        int blockHeight = 5;

        // We expect exactly 50 nibbles of data
        byte[] nibbles = new byte[50];

        // Iterate through the 25 pixels in the block
        for (int i = 0; i < blockWidth * blockHeight; i++)
        {
            int x = i % blockWidth;
            int y = i / blockWidth;

            int nibbleBaseIndex = i * 3;

            // --- Safely break if we have all the data we need ---
            if (nibbleBaseIndex >= 50)
            {
                break;
            }

            int pixelIndex = (startY + y) * width + (startX + x);

            // --- Bounds check before accessing pixel data ---
            if (pixelIndex >= 0 && pixelIndex < requestData.Length)
            {
                Color32 pixel = requestData[pixelIndex];

                // Validate the fixed 0xF pattern in the lower bits
                if ((pixel.r & 0x0F) != 0x0F || (pixel.g & 0x0F) != 0x0F || (pixel.b & 0x0F) != 0x0F)
                {
                    Debug.LogWarning($"Corrupted pattern in pixel ({x},{y}). Might be a compression artifact.");
                }

                // --- Safely extract nibbles, respecting the 50-nibble limit ---
                nibbles[nibbleBaseIndex] = (byte)(pixel.r >> 4);
                if (nibbleBaseIndex + 1 < 50)
                {
                    nibbles[nibbleBaseIndex + 1] = (byte)(pixel.g >> 4);
                }
                if (nibbleBaseIndex + 2 < 50)
                {
                    nibbles[nibbleBaseIndex + 2] = (byte)(pixel.b >> 4);
                }
            }
            else
            {
                Debug.LogError($"Decoder tried to read an out-of-bounds pixel at index: {pixelIndex}");
            }
        }

        // --- Reconstruct bytes from the 50 nibbles ---
        byte[] payload = new byte[25];
        for (int i = 0; i < 50; i += 2)
        {
            int byteIndex = i / 2;
            byte high = nibbles[i];
            byte low = nibbles[i + 1];
            payload[byteIndex] = (byte)((high << 4) | low);
        }

        // --- Verify Checksum ---
        byte receivedChecksum = payload[24];
        byte calculatedChecksum = 0;
        for (int i = 0; i < 24; i++)
        {
            calculatedChecksum ^= payload[i];
        }

        if (receivedChecksum != calculatedChecksum)
        {
            Debug.LogError($"Checksum Mismatch! Data is corrupted. Expected: {receivedChecksum}, Calculated: {calculatedChecksum}. Skipping frame.");
            return;
        }

        // --- Reconstruct Floats ---
        byte[] allBytes = new byte[24];
        Buffer.BlockCopy(payload, 0, allBytes, 0, 24);
        float[] decodedFloats = new float[6];
        for (int i = 0; i < 6; i++)
        {
            decodedFloats[i] = BitConverter.ToSingle(allBytes, i * 4);
        }

        Debug.Log($"Decoded: Pos({decodedFloats[0]:F7}, {decodedFloats[1]:F7}, {decodedFloats[2]:F7}) " +
                  $"Rot({decodedFloats[3]:F7}, {decodedFloats[4]:F7}, {decodedFloats[5]:F7})");

        // Apply to transform
        syncTransform.position = new Vector3(decodedFloats[0], decodedFloats[1], decodedFloats[2]);
        syncTransform.rotation = Quaternion.Euler(decodedFloats[3], decodedFloats[4], decodedFloats[5]);
    }
    // Decodes a 32-bit float from an RGBA32 pixel (with channels in [0,1]).
    private float DecodeFloatFromColor(Color c)
    {
        uint r = (uint)(c.r * 255.0f);
        uint g = (uint)(c.g * 255.0f);
        uint b = (uint)(c.b * 255.0f);
        uint a = (uint)(c.a * 255.0f);
        uint iv = r | (g << 8) | (b << 16) | (a << 24);
        return BitConverter.ToSingle(BitConverter.GetBytes(iv), 0);
    }

    void OnReadTopLeftColor(AsyncGPUReadbackRequest request)
    {
        Vector2Int targetPixel = new Vector2Int(0, 0);
        if (request.hasError)
        {
            Debug.LogError("GPU readback failed");
            return;
        }

        // Get pixel data
        NativeArray<Color32> pixels = request.GetData<Color32>();

        int width = targetTexture.width;
        int height = targetTexture.height;

        // Calculate index (account for Unity's bottom-left origin)
        int x = targetPixel.x;
        int y = targetPixel.y;
        int index = y * width + x;

        // Validate index
        if (index >= 0 && index < pixels.Length)
        {
            Color32 pixelColor32 = pixels[index];

            // Convert Color32 (bytes 0-255 per channel) to Color (floats 0.0-1.0 per channel)
            Color pixelColor = new Color(
                pixelColor32.r / 255.0f,
                pixelColor32.g / 255.0f,
                pixelColor32.b / 255.0f,
                pixelColor32.a / 255.0f
            );

            Debug.Log($"Pixel ({x}, {y}) as Color32: R={pixelColor32.r}, G={pixelColor32.g}, B={pixelColor32.b}, A={pixelColor32.a}");
            Debug.Log($"Pixel ({x}, {y}) as Color (float): {pixelColor}");
        }
    }

    // This method encodes the pose (Vector3 and Quaternion) into 7 floats and writes them into the top row of mirrorTexture.
    private void WritePoseToTexture(Vector3 position, Quaternion rotation)
    {
        int kernel = computeShader.FindKernel("CSMain");

        // Create an array of 7 floats: [pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w]
        float[] poseData = new float[7];
        poseData[0] = position.x;
        poseData[1] = position.y;
        poseData[2] = position.z;
        poseData[3] = rotation.x;
        poseData[4] = rotation.y;
        poseData[5] = rotation.z;
        poseData[6] = rotation.w;

        //Debug.Log(poseData[0]);

        ComputeBuffer poseBuffer = new ComputeBuffer(7, sizeof(float));
        poseBuffer.SetData(poseData);

        // Use targetTexture (writable) as the output texture for pose writing.
        computeShader.SetTexture(kernel, "Result", targetTexture);
        computeShader.SetBuffer(kernel, "PoseBuffer", poseBuffer);

        // Dispatch 7 threads (one per float)
        computeShader.Dispatch(kernel, 7, 1, 1);

        poseBuffer.Release();
    }

    private void UpdatePosAndRot()
    {
        // Get position and rotation (e.g., from a GameObject)
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Debug.Log(rotation);
        // Pass Vector3 to compute shader
        computeShader.SetFloats("_Position", new float[] {
            position.x,
            position.y,
            position.z
        });

        // Pass Quaternion to compute shader
        computeShader.SetFloats("_Rotation", new float[] {
            rotation.x,
            rotation.y,
            rotation.z,
            rotation.w
        });

        computeShader.SetTexture(0, "Source", mirrorTexture);
        computeShader.SetTexture(0, "Result", targetTexture);

        computeShader.Dispatch(0,
            Mathf.CeilToInt(targetTexture.width / 8.0f),
            Mathf.CeilToInt(targetTexture.height / 8.0f),
            1);

        AsyncGPUReadback.Request(targetTexture, 0, OnDataRead);
    }

    private void ModifyPixel()
    {
        if (computeShader == null || targetTexture == null) return;

        // --- 1. Prepare Data Payload (24 bytes data + 1 byte checksum) ---
        float[] floatData = new float[6] {
        syncTransform.position.x,
        syncTransform.position.y,
        syncTransform.position.z,
        syncTransform.rotation.eulerAngles.x,
        syncTransform.rotation.eulerAngles.y,
        syncTransform.rotation.eulerAngles.z
    };

        byte[] allBytes = new byte[24];
        for (int i = 0; i < 6; i++)
        {
            byte[] bytes = BitConverter.GetBytes(floatData[i]);
            Buffer.BlockCopy(bytes, 0, allBytes, i * 4, 4);
        }

        byte checksum = 0;
        foreach (byte b in allBytes) checksum ^= b; // XOR checksum

        byte[] payload = new byte[25];
        Buffer.BlockCopy(allBytes, 0, payload, 0, 24);
        payload[24] = checksum;

        // --- 2. Convert Payload to Nibbles (50 nibbles) ---
        byte[] nibbles = new byte[50];
        for (int i = 0; i < 25; i++)
        {
            nibbles[i * 2] = (byte)((payload[i] >> 4) & 0x0F); // High nibble
            nibbles[i * 2 + 1] = (byte)(payload[i] & 0x0F);       // Low nibble
        }

        // --- 3. Pack Nibbles into a Compute Buffer with proper 4-byte stride ---
        // 50 nibbles / 4 per uint = 12.5 -> 13 uints needed
        uint[] packedNibbles = new uint[13];
        for (int i = 0; i < 13; i++)
        {
            int baseIndex = i * 4;
            // Safely access nibbles array, padding with 0 if we go past the 50th nibble
            packedNibbles[i] =
                (uint)((baseIndex < 50 ? nibbles[baseIndex] : 0) << 24) |
                (uint)((baseIndex + 1 < 50 ? nibbles[baseIndex + 1] : 0) << 16) |
                (uint)((baseIndex + 2 < 50 ? nibbles[baseIndex + 2] : 0) << 8) |
                (uint)(baseIndex + 3 < 50 ? nibbles[baseIndex + 3] : 0);
        }

        ComputeBuffer nibbleBuffer = new ComputeBuffer(13, sizeof(uint));
        nibbleBuffer.SetData(packedNibbles);

        // --- 4. Dispatch Compute Shader ---
        int kernel = computeShader.FindKernel("ModifyColors");
        computeShader.SetTexture(kernel, "Result", targetTexture);
        computeShader.SetBuffer(kernel, "PackedNibbles", nibbleBuffer);
        computeShader.SetInt("startX", 100);
        computeShader.SetInt("startY", 100);
        computeShader.SetInt("texWidth", targetTexture.width);
        computeShader.SetInt("texHeight", targetTexture.height);

        // Use a 5x5 thread group
        computeShader.Dispatch(kernel, 1, 1, 1);

        nibbleBuffer.Release();
    }

    private IEnumerator DebugTextureReadback(RenderTexture texture, int startX, int startY, int width, int height)
    {
        AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(texture);
        yield return new WaitUntil(() => request.done);

        if (request.hasError)
        {
            Debug.LogError("Texture readback failed!");
            yield break;
        }

        Debug.Log("Encoded Block Values:");
        for (int y = 0; y < height; y++)
        {
            string line = "";
            for (int x = 0; x < width; x++)
            {
                int idx = (startY + y) * texture.width + (startX + x);
                Color32 pixel = request.GetData<Color32>()[idx];
                line += $"[{pixel.r:X2},{pixel.g:X2},{pixel.b:X2}] ";

                // Verify encoding pattern
                if ((pixel.r & 0x0F) != 0x0F ||
                    (pixel.g & 0x0F) != 0x0F ||
                    (pixel.b & 0x0F) != 0x0F)
                {
                    Debug.LogError($"Invalid pattern at ({x},{y}): " +
                                   $"{pixel.r & 0x0F:X1},{pixel.g & 0x0F:X1},{pixel.b & 0x0F:X1}");
                }
            }
            Debug.Log(line);
        }
    }
}