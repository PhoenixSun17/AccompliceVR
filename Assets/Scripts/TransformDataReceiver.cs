using UnityEngine;
using Unity.RenderStreaming;
using Unity.WebRTC;
using System;

[AddComponentMenu("Render Streaming/Transform Data Receiver")]
public class TransformDataReceiver : DataChannelBase
{
    [Tooltip("The transform that will be updated by the received data.")]
    public Transform targetTransform;
    public Transform targetTransform2;

    private readonly byte[] _receivedBytes = new byte[24];
    private volatile bool _isMessageReceived = false;

    public override void SetChannel(string connectionId, RTCDataChannel channel)
    {
        // --- DEBUG: Confirm the receiver is being assigned a channel ---
        Debug.Log($"[Receiver] SetChannel called. Channel Label: '{channel.Label}'");

        if (channel != null)
        {
            channel.OnMessage += OnMessageReceived;
        }
    }

    // This method is executed on a BACKGROUND NETWORK THREAD
    private void OnMessageReceived(byte[] bytes)
    {
        // --- DEBUG: This is the most important log. If you see this, the connection is working. ---
        Debug.Log($"[Receiver] OnMessageReceived! Received {bytes.Length} bytes.");

        if (bytes.Length != 24) return;

        lock (_receivedBytes)
        {
            Buffer.BlockCopy(bytes, 0, _receivedBytes, 0, bytes.Length);
        }
        _isMessageReceived = true;
    }

    private void Update()
    {
        if (targetTransform == null) return;

        if (_isMessageReceived)
        {
            // --- DEBUG: Confirm data is being processed on the main thread ---
            Debug.Log("[Receiver] Applying new transform data.");

            float[] transformData = new float[6];
            lock (_receivedBytes)
            {
                Buffer.BlockCopy(_receivedBytes, 0, transformData, 0, _receivedBytes.Length);
            }

            var position = new Vector3(transformData[0], transformData[1], transformData[2]);
            var eulerAngles = new Vector3(transformData[3], transformData[4], transformData[5]);
            Debug.Log($"[Receiver] position is {position}, rotation is {eulerAngles}");
            targetTransform.position = position;
            targetTransform.rotation = Quaternion.Euler(eulerAngles);
            targetTransform2.position = position;
            targetTransform2.rotation = Quaternion.Euler(eulerAngles);
            _isMessageReceived = false;
        }
    }
}