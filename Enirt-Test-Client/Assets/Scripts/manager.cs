using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    netComs net = new netComs();
    public bool enableNetworking;
    public string IPAddress;

    //This script is used to allow uniy to start and stop the asynchronous networking functionalities

    // Start is called before the first frame update
    void Start()
    {
        playerSync.Init();
        net.Init(enableNetworking, IPAddress);
    }

    //Called when the application closes, built in unity event handler
    void OnApplicationQuit()
    {
        playerSync.HaltImmediately();
        netComs.HaltImmediately();
    }

}
