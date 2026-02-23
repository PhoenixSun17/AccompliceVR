using UnityEngine;


[RequireComponent(typeof(PortalScript))] // Require PortalScript component on this GameObject

public class PortalAvatarSelector : MonoBehaviour
{
    // Reference to the GameObject you want to check
    public GameObject targetAvatars;

    private PortalScript targetPortalScript;

    void Awake()
    {
        // Get the PortalScript component attached to this GameObject
        targetPortalScript = GetComponent<PortalScript>();
    }

    void Start()
    {

        // Attempt to get the PortalScript component from the targetObject
        targetPortalScript = GetComponent<PortalScript>();

        // Ensure the PortalScript component exists on the targetObject
        if (targetPortalScript == null)
        {
            Debug.LogError($"PortalScript component is missing on {name}. This is required!");
            return; // Exit if the component is not found
        }
    }

    void Update()
    {
        // Check if targetObject is assigned
        if (targetAvatars != null)
        {

            //Select target
            GameObject targetObject = null;
            if (targetAvatars.transform.childCount > 0)
            {
                foreach (Transform child in targetAvatars.transform)
                {
                    Ubiq.Avatars.Avatar avatar = child.gameObject.GetComponent<Ubiq.Avatars.Avatar>();
                    if (avatar != null & !avatar.IsLocal)
                    {
                        var head = avatar.gameObject.transform.Find("Body/Floating_Head");
                        if (head != null) targetObject = head.gameObject;
                    }
                }
            }

            if (targetObject == null)
                targetObject = targetAvatars;

            targetPortalScript.targetRoot = targetObject;

        }
        else
        {
            Debug.LogWarning("targetAvatars is not assigned.");
        }
    }
}