using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;

[RequireComponent(typeof(Ubiq.Avatars.Avatar))]

public class BlankAvatar : MonoBehaviour
{
    private Ubiq.Avatars.Avatar avatar;
    public GameObject head;
    public GameObject torso;
    public GameObject lhand;
    public GameObject rhand;
    public GameObject portal;

    // Start is called before the first frame update
    void Start()
    {
        avatar = GetComponent<Ubiq.Avatars.Avatar>();
        if (avatar.IsLocal) {
            //head.SetActive(false);
            //torso.SetActive(false);
            //lhand.SetActive(false);
            //rhand.SetActive(false);
            DisableAllRenderers(head);
            DisableAllRenderers(torso);
            DisableAllRenderers(lhand);
            DisableAllRenderers(rhand);
        }
    }

        // Call this method to disable all Renderer components on the target GameObject and its descendants
    public void DisableAllRenderers(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not set.");
            return;
        }

        // Disable all renderers on the target and recursively on all children
        DisableRenderersRecursive(targetObject);
    }

    // Recursive method to disable all Renderer components on the given GameObject and its children
    private void DisableRenderersRecursive(GameObject obj)
    {
        // Get all Renderer components on the current object
        Renderer[] renderers = obj.GetComponents<Renderer>();

        // Disable each Renderer on the current object
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Recursively disable Renderers on all child objects
        foreach (Transform child in obj.transform)
        {
            DisableRenderersRecursive(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if (portal == null)
        {
            portal = GameObject.FindWithTag("Portal");
        }
        if (portal != null)
        {
            this.transform.rotation = portal.transform.rotation;
        }*/
        
    }
}
