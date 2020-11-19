using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    readonly NetComs net = new NetComs();
    public bool enableNetworking;

    public static int Width;
    public static int Height;
    public static bool keyboardEnable;

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
        PlayerSync.Init();
        OrbSync.Init();
        net.Init(enableNetworking);

        PlayerData inital = new PlayerData(20, 0, 0, 0);
        PlayerManager.AddPlayer(inital, 20, false);
    }

    void Update()
    {
        //If we are dead, make us respawn
        if(PlayerManager.ourPlayers.Count == 0)
        {
            PlayerManager.ResetFile();
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
            PlayerData inital = new PlayerData(20, 0, 0, 0);
            PlayerManager.AddPlayer(inital, 20, false);
        }

        //If we push escape, take us to the menu
        if (Input.GetKeyDown("escape"))
        {
            LoadGame.errorText = "";
            PlayerSync.HaltImmediately();
            NetComs.HaltImmediately();
            OrbSync.HaltImmediately();
            SceneManager.LoadScene("splash");
        }
    }

    //Called when the application closes, built in unity event handler
    void OnApplicationQuit()
    {
        PlayerSync.HaltImmediately();
        NetComs.HaltImmediately();
        OrbSync.HaltImmediately();
    }

    //Called by other classes to make the game return to the menu screen
    public static void ErrorReturnToMenu()
    {
        PlayerSync.HaltImmediately();
        NetComs.HaltImmediately();
        OrbSync.HaltImmediately();
        SceneManager.LoadScene("splash");
    }
}
