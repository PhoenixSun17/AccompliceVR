using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;
using Ubiq.Messaging;

public class LineController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Line;
    NetworkContext context;

    private void Start()
    {
        context = NetworkScene.Register(this);
        Line = this.GetComponentInChildren<LineRenderer>().gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeLine();
        }
    }

    public void SendMessage(bool isLine)
    {
        context.SendJson(new Message()
        {
            isLineRenderred = isLine
        }); 
    }

    private struct Message
    {
        public bool isLineRenderred;
    }

    private void ChangeLine()
    {
        if (Line.activeSelf)
        {
            Line.active = false;
            SendMessage(false);
        }
        else
        {
            Line.active = true;
            SendMessage(true);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        Line.active = m.isLineRenderred;


    }
}
