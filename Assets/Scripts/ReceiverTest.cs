using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.RenderStreaming;

class ReceiverTest : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private SignalingManager renderStreaming;
    [SerializeField] private RawImage remoteVideoImage;
    [SerializeField] private AudioSource remoteAudioSource;
    [SerializeField] private VideoStreamReceiver receiveVideoViewer;
    [SerializeField] private AudioStreamReceiver receiveAudioViewer;
    [SerializeField] private SingleConnection connection;
#pragma warning restore 0649

    private string connectionId;
    private InputSender inputSender;
    private Vector2 lastSize;
    private bool isReconnecting = false;
    private float reconnectDelay = 5f; // Time delay between reconnection attempts

    void Awake() {
        receiveVideoViewer.OnUpdateReceiveTexture += OnUpdateReceiveTexture;
        receiveAudioViewer.OnUpdateReceiveAudioSource += source => {
            source.loop = true;
            source.Play();
        };

        inputSender = GetComponent<InputSender>();
        inputSender.OnStartedChannel += OnStartedChannel;
    }

    void Start() {
        if (renderStreaming.runOnAwake)
            return;
        renderStreaming.Run();
        Invoke("OnStart", 2);
    }

    private void Update() {
        // Call SetInputChange if window size is changed.
        var size = remoteVideoImage.rectTransform.sizeDelta;
        if (lastSize != size) {
            lastSize = size;
            CalculateInputRegion();
        }

        // Monitor the connection status and attempt reconnection if necessary
        if (!string.IsNullOrEmpty(connectionId) && !connection.IsConnected(connectionId) && !isReconnecting) {
            StartCoroutine(TryReconnect());
        }
    }

    void OnUpdateReceiveTexture(Texture texture) {
        remoteVideoImage.texture = texture;
        CalculateInputRegion();
    }

    void OnStartedChannel(string connectionId) {
        CalculateInputRegion();
    }

    private void OnRectTransformDimensionsChange() {
        CalculateInputRegion();
    }

    void CalculateInputRegion() {
        if (inputSender == null || !inputSender.IsConnected || remoteVideoImage.texture == null)
            return;
    }

    private void OnStart() {
        CreateNewConnection();
    }

    private void OnStop() {
        connection.DeleteConnection(connectionId);
        connectionId = String.Empty;
    }

    private void CreateNewConnection() {
        connectionId = System.Guid.NewGuid().ToString("N");
        receiveAudioViewer.targetAudioSource = remoteAudioSource;
        connection.CreateConnection(connectionId);
    }

    private IEnumerator TryReconnect() {
        isReconnecting = true;
        Debug.Log("Connection lost. Attempting to reconnect...");
        
        yield return new WaitForSeconds(reconnectDelay);

        CreateNewConnection();

        // Wait a bit after attempting to reconnect, then stop reconnecting
        yield return new WaitForSeconds(2);
        isReconnecting = false;
    }
}
