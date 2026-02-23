using System.Collections;
using System.Collections.Generic;
using Ubiq.XR;
using UnityEngine;

public class AudioBox : MonoBehaviour, IUseable, IGraspable
{
    private AudioSource audioSource;
    private Hand follow;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Grasp(Hand controller)
    {
        follow = controller;
    }

    public void Release(Hand controller)
    {
        follow = null;
    }

    public void UnUse(Hand controller)
    {

    }

    public void Use(Hand controller)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.Play();
        }
    }

    private void Update()
    {

    }
}
