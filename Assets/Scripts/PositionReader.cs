using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.UI;
using Valve.VR;
using System;

public class PositionReader : MonoBehaviour
{
    // Start is called before the first frame update
    public RenderTexture targetTexture;
    public Vector3 position;
    public Vector3 rotation;
    public Transform projector;
    public Transform wiper;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AsyncGPUReadback.Request(targetTexture, 0, TextureFormat.RGBA32, OnReadColor);
        //projector.transform.position = position;
        //Debug.Log("p pos = " + position);
        //wiper.transform.position = position;
        //Debug.Log("w pos = " + rotation);
        //projector.transform.eulerAngles = rotation;
        //wiper.transform.eulerAngles = rotation;
    }

    void OnReadColor(AsyncGPUReadbackRequest request)
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
        position = new Vector3(decodedFloats[0], decodedFloats[1], decodedFloats[2]);
        rotation = new Vector3(decodedFloats[3], decodedFloats[4], decodedFloats[5]);
    }
}
