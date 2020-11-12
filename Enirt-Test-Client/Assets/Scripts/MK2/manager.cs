using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    netComs net = new netComs();
    public bool enableNetworking;
    public string IPAddress;

    public static int Width;
    public static int Height;

    public int boardWidth;
    public int boardHeight;


    //This script is used to allow uniy to start and stop the asynchronous networking functionalities

    // Start is called before the first frame update
    void Start()
    {
        Width = boardWidth;
        Height = boardHeight;
        playerSync.Init();
        orbSync.Init();
        net.Init(enableNetworking, IPAddress);
    }

    private void Update()
    {
        
    }

    //Called when the application closes, built in unity event handler
    void OnApplicationQuit()
    {
        playerSync.HaltImmediately();
        netComs.HaltImmediately();
        orbSync.HaltImmediately();
    }

}
