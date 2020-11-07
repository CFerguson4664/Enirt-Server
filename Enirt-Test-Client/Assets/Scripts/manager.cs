using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    netComs net = new netComs();
    public bool enableNetworking;
    public string IPAddress;
    // Start is called before the first frame update
    void Start()
    {
        playerSync.Init();
        net.Init(enableNetworking, IPAddress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        playerSync.HaltImmediately();
        netComs.HaltImmediately();
    }

}
