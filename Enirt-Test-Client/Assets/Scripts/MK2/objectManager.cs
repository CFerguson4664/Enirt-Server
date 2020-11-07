﻿using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class objectManager : MonoBehaviour
{
    //The template object used to represent other players
    public static GameObject marker;
    public GameObject markerObj;

    //Store the current data for each player in a dictionary indexed with the player ids.
    //The data in this is updated by playerSync.
    public static Dictionary<int,PlayerData> players = new Dictionary<int,PlayerData>();

    // Start is called before the first frame update
    void Start()
    {
        marker = markerObj;
    }

    // Update is called once per frame
    void Update()
    {
        //Update the graphics for every opponent
        foreach (PlayerData player in players.Values)
        {
            interpolatePlayer(player);
        }
    }


    static void interpolatePlayer(PlayerData player)
    {
        long time = netComs.GetTime();
        //Time since the position of the player was last received
        long timeSinceData = time - player.Time;

        //Finding velocity of the player
        float xTravel = player.XPos - player.LastXPos;
        float yTravel = player.YPos - player.LastYPos;
        long timeTravel = player.Time - player.LastTime;
        float xVelocity = xTravel / timeTravel;
        float yVelocity = yTravel / timeTravel;

        //Estimate the position of the player now
        float xPos = player.XPos + (xVelocity * timeSinceData);
        float yPos = player.YPos + (yVelocity * timeSinceData);
        float zPos = player.ZPos;

        //Check if the player has a marker associated with them
        if(player.gameObject != null)
        {
            //If they do update its position
            player.gameObject.transform.position = new Vector3(xPos, yPos, zPos);
            player.gameObject.GetComponent<eatDots>().size = player.Size;
        }
        else
        {
            //If not create one
            player.gameObject = Instantiate(marker, new Vector3(xPos, yPos, zPos), Quaternion.identity);
            player.gameObject.GetComponent<eatDots>().size = player.Size;
        }
        
    }
}

public class PlayerData
{
    //Store the playerId of the player
    public int Id { get; set; }

    //Store the time that this data was updated
    public long Time { get; set; }
    //Store the previoustime that this data was updateda
    public long LastTime { get; set; }

    //The size of the player 
    public int Size { get; set; }

    //The x and y position of the player
    public float XPos { get; set; }
    public float YPos { get; set; }

    //The previous position of this player
    public float LastXPos { get; set; }
    public float LastYPos { get; set; }

    //The Z position of the player, this should probably be 0
    public float ZPos { get; set; }

    //The game object representing this player
    public GameObject gameObject { get; set; }

    public PlayerData()
    {
        gameObject = null;
    }

    public PlayerData(int idIn, long timeIn, int sizeIn, float xPosIn, float yPosIn, float zPosIn)
    {
        Id = idIn;
        Time = timeIn;
        Size = sizeIn;
        XPos = xPosIn;
        YPos = yPosIn;
        ZPos = zPosIn;
        LastXPos = xPosIn;
        LastYPos = yPosIn;
        gameObject = null;
    }

    public void UpdateData(long time, int size, float x, float y, float z)
    {
        Size = size;
        LastXPos = XPos;
        LastYPos = YPos;
        Time = time;
        XPos = x;
        YPos = y;
        ZPos = z;
    }
}