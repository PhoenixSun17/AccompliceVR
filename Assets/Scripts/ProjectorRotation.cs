using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem.Sample;

public class ProjectorRotation : MonoBehaviour
{
    // Start is called before the first frame update
    public AvatarTracker tracker;
    public GameObject trackingAvatar;
    [Range(1, 20)]
    public int delayRange;
    public Queue<Transform> transforms = new Queue<Transform>();
    public Transform temp;

    void Start()
    {
        Debug.Log($"Now the system have {delayRange} latency");
        for (int i = 0; i<delayRange; i++)
        {
            transforms.Enqueue(transform);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trackingAvatar == null)
        {
            trackingAvatar = GameObject.FindWithTag("Head");
        }
        else
        {
            temp = transforms.Dequeue();
            transforms.Enqueue(trackingAvatar.transform);
            transform.localRotation = temp.rotation;
            transform.localPosition = temp.position;
            //transform.Rotate(0, 0, 180);
        }


    }

}
