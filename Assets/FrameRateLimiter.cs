using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] public int targetFrameRate = 60;

    void Start()
    {
        // Set the target frame rate
        Application.targetFrameRate = targetFrameRate;
    }
}