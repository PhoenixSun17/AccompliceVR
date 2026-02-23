using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class FrameAlignment : MonoBehaviour
{
    NetworkContext context;

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
    }

    Vector3 lastPosition;

    // Update is called once per frame
    public void SendMessage(int num, Vector3 pos, Quaternion rot)
    {
        Debug.Log("Sending position");
        context.SendJson(new Message()
        {
            number = num,
            position = pos,
            rotation = rot
        });
    }

    private struct Message
    {
        public int number;
        public Vector3 position;
        public Quaternion rotation;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;


    }
}
