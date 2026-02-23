using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderCubeMap : MonoBehaviour
{
    public Camera targetCamera;
    public Camera wipeCamera;
    public Camera projectCamera;
    public Cubemap cubemap;
    public GameObject world;
    public GameObject proxy;
    public Transform HeadPos;

    private int frameCounter;
    private Transform LegacyPos;
    private Queue<Transform> Positions = new Queue<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        frameCounter = 0;
        Positions.Enqueue(HeadPos);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Positions.Count < 11)
        {
            Positions.Enqueue(HeadPos);
        }
        if (Positions.Count > 10)
        {
            LegacyPos = Positions.Dequeue();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Render cubemap");
            wipeCamera.RenderToCubemap(cubemap);
        }
        if (frameCounter == 10) {
            frameCounter = 0;
            projectCamera.transform.position = LegacyPos.position;
            targetCamera.RenderToCubemap(cubemap);
        }
        frameCounter++;*/
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Render cubemap");
            wipeCamera.RenderToCubemap(cubemap);
        }
        targetCamera.RenderToCubemap(cubemap);
    }
}
