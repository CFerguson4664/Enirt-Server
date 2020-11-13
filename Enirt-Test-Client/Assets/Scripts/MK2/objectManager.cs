﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using UnityEngine;

public class objectManager : MonoBehaviour
{
    //The template object used to represent other players
    public static GameObject marker;
    public GameObject markerObj;

    public static GameObject orb;
    public GameObject orbObj;

    //Store the player data from each client in a dictionary indexed with the player ids.
    public static Dictionary<int, ClientData> currentClients = new Dictionary<int, ClientData>();

    //Stores the ids of the orbs to be removed the next cycle
    public static List<PlayerData> addPlayers = new List<PlayerData>();
    private static List<PlayerData> addPlayersActive = new List<PlayerData>();

    //Stores the ids of the orbs to be added the next cycle
    public static List<IdPair> removePlayers = new List<IdPair>();
    private static List<IdPair> removePlayersActive = new List<IdPair>();



    //Stores the orbs currently being displayed in a dictionary indexed with the orb timestamps.
    public static Dictionary<long, OrbData> currentOrbs = new Dictionary<long, OrbData>();

    //Stores the ids of the orbs to be removed the next cycle
    public static List<OrbData> addOrbs = new List<OrbData>();
    private static List<OrbData> addOrbsActive = new List<OrbData>();

    //Stores the ids of the orbs to be added the next cycle
    public static List<long> removeOrbs = new List<long>();
    private static List<long> removeOrbsActive = new List<long>();

    public static void ResetFile()
    {
        //Store the player data from each client in a dictionary indexed with the player ids.
        currentClients = new Dictionary<int, ClientData>();

        //Stores the ids of the orbs to be removed the next cycle
        addPlayers = new List<PlayerData>();
        addPlayersActive = new List<PlayerData>();

        //Stores the ids of the orbs to be added the next cycle
        removePlayers = new List<IdPair>();
        removePlayersActive = new List<IdPair>();



        //Stores the orbs currently being displayed in a dictionary indexed with the orb timestamps.
        currentOrbs = new Dictionary<long, OrbData>();

        //Stores the ids of the orbs to be removed the next cycle
        addOrbs = new List<OrbData>();
        addOrbsActive = new List<OrbData>();

        //Stores the ids of the orbs to be added the next cycle
        removeOrbs = new List<long>();
        removeOrbsActive = new List<long>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetFile();
        orb = orbObj;
        marker = markerObj;
    }

    // Update is called once per frame
    void Update()
    {
        AddPlayers();
        RemovePlayers();
        ProcessPlayers();

        AddOrbs();
        RemoveOrbs();
    }

    static void AddPlayers()
    {
        addPlayersActive = addPlayers;
        addPlayers = new List<PlayerData>();

        foreach (PlayerData playerData in addPlayersActive)
        {
            if (!currentClients.ContainsKey(playerData.ClientId))
            {
                currentClients.Add(playerData.ClientId, new ClientData(playerData.ClientId));
            }

            if(!currentClients[playerData.ClientId].Players.ContainsKey(playerData.Id))
            {
                //If not create one
                playerData.gameObject = Instantiate(marker, new Vector3(playerData.XPos, playerData.YPos, playerData.ZPos), Quaternion.identity);
                playerData.gameObject.GetComponent<markerEatDots>().size = playerData.Size;
                playerData.gameObject.GetComponent<markerEatDots>().clientId = playerData.ClientId;
                playerData.gameObject.GetComponent<markerEatDots>().playerId = playerData.Id;
                currentClients[playerData.ClientId].Players.Add(playerData.Id, playerData);
            }
        }
    }

    static void RemovePlayers()
    {
        removePlayersActive = removePlayers;
        removePlayers = new List<IdPair>();

        foreach (IdPair Ids in removePlayersActive)
        {
            ClientData client = currentClients[Ids.ClientId];

            if (client.Players.ContainsKey(Ids.PlayerId))
            {
                if (client.Players[Ids.PlayerId].gameObject != null)
                {
                    Destroy(client.Players[Ids.PlayerId].gameObject);
                }
                client.Players.Remove(Ids.PlayerId);
            }
        }
    }

    static void ProcessPlayers()
    {
        foreach (ClientData client in currentClients.Values)
        {
            foreach (PlayerData player in client.Players.Values)
            {
                InterpolatePlayer(player);
            }
        }
    }

    static void AddOrbs()
    {
        addOrbsActive = addOrbs;
        addOrbs = new List<OrbData>();

        long time = netComs.GetTime();

        int counter = 0;

        foreach (OrbData orbData in addOrbsActive)
        {
            if(counter > 5)
            {
                addOrbs.Concat(addOrbsActive);
                break;
            }

            if(orbData.Id < time)
            {
                if (!currentOrbs.ContainsKey(orbData.Id))
                {
                    orbData.gameObject = Instantiate(orb, new Vector3(orbData.XPos, orbData.YPos, orbData.ZPos), Quaternion.identity);
                    orbData.gameObject.GetComponent<orbData>().Id = orbData.Id;
                    currentOrbs.Add(orbData.Id, orbData);
                }
                else
                {
                    addOrbs.Add(orbData);
                }
            }
            else
            {
                addOrbs.Add(orbData);
            }

            counter++;
        }
    }

    static void RemoveOrbs()
    {
        removeOrbsActive = removeOrbs;
        removeOrbs = new List<long>();

        int counter = 0;

        foreach (long Id in removeOrbsActive)
        {
            if (counter > 5)
            {
                removeOrbs.Concat(removeOrbsActive);
                break;
            }

            if (currentOrbs.ContainsKey(Id))
            {
                if(currentOrbs[Id].gameObject != null) {
                    Destroy(currentOrbs[Id].gameObject);
                }
                currentOrbs.Remove(Id);
            }

            counter++;
        }
    }

    static void InterpolatePlayer(PlayerData player)
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
            player.gameObject.GetComponent<markerEatDots>().size = player.Size;
        }
        else
        {
            //If not create one
            player.gameObject = Instantiate(marker, new Vector3(xPos, yPos, zPos), Quaternion.identity);
            player.gameObject.GetComponent<markerEatDots>().size = player.Size;
        }
    }
}

public class PlayerData
{
    //Store the playerId of the player
    public int Id { get; set; }
    public int ClientId { get; set; }

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

    public PlayerData(int sizeIn, float xPosIn, float yPosIn, float zPosIn)
    {
        Size = sizeIn;
        XPos = xPosIn;
        YPos = yPosIn;
        ZPos = zPosIn;
        gameObject = null;
    }

    public PlayerData(int idIn, int clientIdIn, long timeIn, int sizeIn, float xPosIn, float yPosIn, float zPosIn)
    {
        Id = idIn;
        ClientId = clientIdIn;
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

public class OrbData
{
    public long Id { get; set; }
    public float XPos { get; set; }
    public float YPos { get; set; }
    public float ZPos { get; set; }
    public GameObject gameObject { get; set; }

    public OrbData()
    {

    }

    public OrbData(long IdIn, float XPosIn, float YPosIn)
    {
        Id = IdIn;
        XPos = XPosIn;
        YPos = YPosIn;
        ZPos = 0;
    }
}

public class ClientData
{
    public int Id { get; set; }

    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

    public ClientData()
    {

    }

    public ClientData(int IdIn)
    {
        Id = IdIn;
    }
}

public class IdPair
{
    public int ClientId { get; set; }
    public int PlayerId { get; set; }

    public IdPair()
    {

    }

    public IdPair(int clientIdIn, int playerIdIn)
    {
        ClientId = clientIdIn;
        PlayerId = playerIdIn;
    }
}

