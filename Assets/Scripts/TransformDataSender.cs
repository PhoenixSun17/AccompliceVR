using UnityEngine;
using Unity.RenderStreaming;
using Unity.WebRTC;
using System.Collections.Generic;
using System;

[AddComponentMenu("Render Streaming/Transform Data Sender")]
public class TransformDataSender : DataChannelBase
{
    [Tooltip("The transform to track and send over the data channel.")]
    public Transform targetTransform;

    private RTCDataChannel _channel;

    public override void SetChannel(string connectionId, RTCDataChannel channel)
    {
        // --- DEBUG: Confirm that the system is giving us a channel ---
        Debug.Log($"[Sender] SetChannel called for connection ID: {connectionId}");

        if (_channel != null)
        {
            _channel.OnOpen -= OnOpen;
            _channel.OnClose -= OnClose;
        }

        _channel = channel;
        if (_channel == null) return;

        _channel.OnOpen += OnOpen;
        _channel.OnClose += OnClose;
    }

    private void OnOpen()
    {
        // --- DEBUG: Confirm the channel is successfully opened ---
        Debug.Log("[Sender] Data Channel is OPEN.");
    }

    private void OnClose()
    {
        // --- DEBUG: Confirm the channel has closed ---
        Debug.Log("[Sender] Data Channel is CLOSED.");
        //_channel = null;
    }

    private void Update()
    {
        if (targetTransform == null) return;
        if (_channel == null || _channel.ReadyState != RTCDataChannelState.Open) return;

        // --- DEBUG: Confirm we are attempting to send data ---
        Debug.Log("[Sender] Sending transform data...");
        try
        {
            // Serialize transform data
            float[] transformData = new float[6] {
                targetTransform.position.x, targetTransform.position.y, targetTransform.position.z,
                targetTransform.rotation.eulerAngles.x, targetTransform.rotation.eulerAngles.y, targetTransform.rotation.eulerAngles.z
            };
            byte[] bytes = new byte[transformData.Length * sizeof(float)];
            Buffer.BlockCopy(transformData, 0, bytes, 0, bytes.Length);

            // Send data
            _channel.Send(bytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Sender] Exception while sending data: {e.Message}\n{e.StackTrace}");
        }
        
    }
}