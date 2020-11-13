using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class manager : MonoBehaviour
{
    netComs net = new netComs();
    public bool enableNetworking;

    public static int Width;
    public static int Height;

    public int boardWidth;
    public int boardHeight;

    private void Awake()
    {
        
    }

    //This script is used to allow uniy to start and stop the asynchronous networking functionalities

    // Start is called before the first frame update
    void Start()
    {
        Width = boardWidth;
        Height = boardHeight;
        playerSync.Init();
        orbSync.Init();
        net.Init(enableNetworking);

        PlayerData inital = new PlayerData(20, 0, 0, 0);
        playerManager.AddPlayer(inital, 20, false);
    }

    void Update()
    {
        if(playerManager.ourPlayers.Count == 0)
        {
            playerManager.ResetFile();
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
            PlayerData inital = new PlayerData(20, 0, 0, 0);
            playerManager.AddPlayer(inital, 20, false);
        }


        if (Input.GetKeyDown("escape"))
        {
            playerSync.HaltImmediately();
            netComs.HaltImmediately();
            orbSync.HaltImmediately();
            SceneManager.LoadScene("splash");
        }
    }

    //Called when the application closes, built in unity event handler
    void OnApplicationQuit()
    {
        playerSync.HaltImmediately();
        netComs.HaltImmediately();
        orbSync.HaltImmediately();
    }

}
