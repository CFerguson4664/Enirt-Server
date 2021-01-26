using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class manager : MonoBehaviour
{
    readonly netComs net = new netComs();
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
        playerSync.Init();
        orbSync.Init();
        net.Init(enableNetworking);

        PlayerData inital = new PlayerData(20, 0, 0, 0);
        playerManager.AddPlayer(inital, 20, false);
    }

    void Update()
    {
        //If we are dead, make us respawn
        if(playerManager.ourPlayers.Count == 0)
        {
            playerManager.ResetFile();
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
            PlayerData inital = new PlayerData(20, 0, 0, 0);
            playerManager.AddPlayer(inital, 20, false);
        }

        //If we push escape, take us to the menu
        if (Input.GetKeyDown("escape"))
        {
            loadGame.errorText = "";
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

    //Called by other classes to make the game return to the menu screen
    public static void ErrorReturnToMenu()
    {
        playerSync.HaltImmediately();
        netComs.HaltImmediately();
        orbSync.HaltImmediately();
        SceneManager.LoadScene("splash");
    }
}
