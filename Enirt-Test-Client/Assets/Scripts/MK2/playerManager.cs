using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class playerManager : MonoBehaviour
{
    public static float cameraSpeed = 10;
    public static float cameraSlack = 2;
    public static float cameraZoomSpeed = 0.5f;

    public static Dictionary<int, Player> ourPlayers = new Dictionary<int, Player>();
    public static GameObject playerPrefab;
    public GameObject playerObj;

    public static void ResetFile()
    {
        ourPlayers = new Dictionary<int, Player>();
    }

    void Awake()
    {
        ResetFile();
    }

    //Adds players to the game instance
    public static void AddPlayer(PlayerData playerData, int size, bool glide)
    {
        //Check to make sure the player hasnt already been added
        int playerId = 0;
        for(int i = 0; i < int.MaxValue; i++)
        {
            if(!ourPlayers.ContainsKey(i))
            {
                playerId = i;
                break;
            }
        }

        //Create a player from the provided PlayerData
        Player player = new Player(playerData);

        //Instatiate the player obejct and set its parameters
        player.gameObject = Instantiate(playerPrefab, new Vector3(player.XPos, player.YPos, player.ZPos), Quaternion.identity);
        player.gameObject.GetComponent<EatDots>().size = size;
        player.gameObject.GetComponent<PlayerMovement>().Id = playerId;
        player.gameObject.GetComponent<PlayerMovement>().name.text = playerData.name;
        player.gameObject.GetComponent<SpriteRenderer>().color = playerData.color;

        player.SetRecombine();
        player.Id = playerId;

        //If the object needs to glide, let i know
        if(glide)
        {
            player.gameObject.GetComponent<PlayerMovement>().StartGlide();
        }

        //Add the new player to our list of player
        ourPlayers.Add(playerId, player);
        scoreboard.addScoreboardPlayer(player);

    }

    public static void RemovePlayer(int Id)
    {
        //Remove the player with the specified id from our list and delete their object in game
        if (ourPlayers.ContainsKey(Id))
        {
            if (ourPlayers[Id].gameObject != null)
            {
                Destroy(ourPlayers[Id].gameObject);
            }
            ourPlayers.Remove(Id);
            scoreboard.removeScoreboardPlayer(Id);
        }
    }


    // Start is called before the first frame update
    //Create the initial player object
    void Start()
    {
        playerPrefab = playerObj;
    }

    // Update is called once per frame
    //Look to see if any orbs can recombine
    void FixedUpdate()
    {
        CheckRecombines();
    }

    //Move the in game camera
    void Update()
    {
        MoveCamera();
    }

    void CheckRecombines()
    {
        //Look at every player object
        var keys = ourPlayers.Keys.ToArray();

        foreach (int key in keys)
        {
            if (ourPlayers.ContainsKey(key))
            {
                
                Player player = ourPlayers[key];
                if (player.currentPlayerCollisions.Count > 0)
                {
                    //Look at every collision that player is experiencing
                    foreach (int i in player.currentPlayerCollisions)
                    {

                        //Look at every object the player is colliding with
                        if(ourPlayers.ContainsKey(i))
                        {
                            Player otherPlayer = ourPlayers[i];

                            //Recombine if both objects are ready
                            if(player.recombineTime <= 0 && otherPlayer.recombineTime <= 0)
                            {
                                player.gameObject.GetComponent<EatDots>().size += otherPlayer.gameObject.GetComponent<EatDots>().size;
                                player.SetRecombine();
                                if (manager.keyboardEnable)
                                {
                                    if (otherPlayer.Id == PlayerMovement.smallestPlayer.Id)
                                    {
                                        PlayerMovement.smallestPlayer = player;
                                    }
                                }
                                RemovePlayer(otherPlayer.Id);
                            }
                        }
                    }
                }

                //Adjust the recombine timer
                if(player.recombineTime > 0)
                {
                    player.recombineTime -= Time.fixedDeltaTime;
                }
            }
        }
    }

    void MoveCamera()
    {
        //Takes a weighted average of the player positions
        var keys = ourPlayers.Keys.ToArray();

        float totalX = 0;
        float totalY = 0;
        float totalS = 0;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        float minY = float.MaxValue;
        float maxY = float.MinValue;


        //Calculate weighted average position
        foreach (int key in keys)
        {
            if(ourPlayers.ContainsKey(key))
            {
                Player player = ourPlayers[key];
                totalX += player.XPos * player.Size;
                totalY += player.YPos * player.Size;
                totalS += player.Size;

                float radius = Mathf.Sqrt(player.Size / 50f / Mathf.PI);

                if (player.XPos + radius > maxX)
                {
                    maxX = player.XPos + radius;
                }
                if(player.XPos - radius < minX)
                {
                    minX = player.XPos - radius;
                }
                if (player.YPos + radius> maxY)
                {
                    maxY = player.YPos + radius;
                }
                if (player.YPos - radius < minY)
                {
                    minY = player.YPos - radius;
                }
            }
        }
        float wAvgX = totalX / totalS;
        float wAvgY = totalY / totalS;

        //Zoom the camera based on the player's size
        float cameraViewHeight = Camera.main.orthographicSize * 2;
        float cameraViewWidth = cameraViewHeight * Camera.main.aspect;

        if (keys.Length == 0)
        {
            wAvgX = 0;
            wAvgY = 0;
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;
        }

        float offsetX = maxX - minX;
        float offsetY = maxY - minY;

        //Adjust the camera's viewport
        if(offsetX >= cameraViewWidth * 0.6 || offsetY >= cameraViewHeight * 0.6)
        {
            Camera.main.orthographicSize += cameraZoomSpeed * Time.deltaTime;
        }
        else
        {
            if(Camera.main.orthographicSize > 5)
            {
                Camera.main.orthographicSize -= cameraZoomSpeed * Time.deltaTime;
            }
        }

        //Position the camera
        Vector3 cameraGoalPos = new Vector3(wAvgX, wAvgY, Camera.main.transform.position.z);

        float cameraSpeedAdj = Vector2.Distance(Camera.main.transform.position, cameraGoalPos) * cameraSpeed * (1 / cameraSlack) * Time.deltaTime;

        Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, cameraGoalPos, cameraSpeedAdj);
    }
}

public class Player : PlayerData
{
    public List<int> currentPlayerCollisions = new List<int>();

    public double recombineTime = 1;

    public void SetRecombine()
    {
        recombineTime = (Math.Pow(Math.Log(Size * 1d + 2), 1.7) * 3);
    }

    public Player(int sizeIn, float xIn, float yIn, float zIn, string clientData, Color clientColor) : base(sizeIn, xIn, yIn, zIn, clientData, clientColor)
    {

    }

    public Player(PlayerData data) : base(data.Size, data.XPos, data.YPos, data.ZPos, data.name, data.color)
    {

    }
}
