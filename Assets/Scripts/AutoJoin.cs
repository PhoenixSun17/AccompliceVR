
using System;
using Ubiq.Rooms;
using UnityEngine;

[RequireComponent(typeof(RoomClient))]

public class AutoJoin : MonoBehaviour
{
//    public bool JoinOnStart = false;
    public string Guid = null;

    private void Start() {
        if (Guid == null || Guid == "") {
            Guid = System.Guid.NewGuid().ToString();
        }

        GetComponent<RoomClient>().Join(System.Guid.Parse(Guid));
    }
}