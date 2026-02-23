using Unity.RenderStreaming;
using Unity.WebRTC;
using UnityEngine;

[RequireComponent(typeof(VideoStreamSender))]
public class EnhancedVideoSender : MonoBehaviour
{
    private VideoStreamSender videoSender;
    private RTCDataChannel dataChannel;

    /*private void Awake()
    {
        videoSender = GetComponent<VideoStreamSender>();
        videoSender.OnStartedStream += OnStreamStarted;
    }

    private void OnStreamStarted(string connectionId)
    {
        // Access the peer connection through the signaling handler
        var handler = videoSender.GetComponentInParent<SignalingHandlerBase>();
        var peerConnection = handler.GetPeerConnection(connectionId);

        // Create the data channel
        dataChannel = peerConnection.CreateDataChannel("floatData");
        dataChannel.OnOpen += () => Debug.Log("Data channel ready");
    }*/

    public void SendFloat(float value)
    {
        if (dataChannel?.ReadyState == RTCDataChannelState.Open)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            dataChannel.Send(bytes);
            Debug.Log($"Sent float: {value}");
        }
    }
}