using Unity.WebRTC;
using UnityEngine;
using System.Collections;

public class FloatReceiver : MonoBehaviour
{
    private RTCPeerConnection peerConnection;
    public float receivedFloat;

    private void Start()
    {
        //WebRTC.Initialize();

        var config = new RTCConfiguration { iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } } };
        peerConnection = new RTCPeerConnection(ref config);

        // Listen for incoming data channel
        peerConnection.OnDataChannel = (channel) =>
        {
            channel.OnMessage = (byte[] data) =>
            {
                // Convert bytes back to float (exact same value)
                receivedFloat = System.BitConverter.ToSingle(data, 0);
                Debug.Log($"Received float: {receivedFloat}");
            };
        };

        // Start signaling (exchange SDP/ICE candidates with sender)
        StartCoroutine(SetupPeerConnection());
    }

    private void Update()
    {
        Debug.Log($"Received float????: {receivedFloat}");

    }

    private IEnumerator SetupPeerConnection()
    {
        // Wait for offer from sender, set remote description, create answer, etc.
        // (Depends on your signaling implementation)
        yield return null;
    }

    private void OnDestroy()
    {
        peerConnection?.Close();
        //WebRTC.Dispose();
    }
}