using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class SharedHMD : MonoBehaviour
{
    private NetworkContext context;

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public Message(Transform transform) {
            this.position = transform.position;
            this.rotation = transform.rotation;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg) {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    private void FixedUpdate() {
        context.SendJson(new Message());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
