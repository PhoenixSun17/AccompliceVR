using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarTracker : MonoBehaviour
{
    public GameObject avatars;
    public GameObject trackingAvatar;
    private GameObject trackingHead;

    // Start is called before the first frame update
    void Start()
    {
        trackingAvatar = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (trackingAvatar==null || !trackingAvatar.activeInHierarchy) {
            BlankAvatar avatar = avatars.GetComponentInChildren<BlankAvatar>();
            if (avatar != null) { 
                trackingAvatar = avatar.gameObject;
                trackingHead = avatar.transform.Find("Body/Floating_Head").gameObject;
            }
        }
        if (trackingAvatar!=null && trackingHead!=null) {
            transform.position = trackingHead.transform.position;
            transform.rotation = trackingHead.transform.rotation;
        }
    }
}
