using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Timers;
using System.Threading;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public class playerSync
{
    static bool active = false;
    static System.Timers.Timer syncTimer;
    static Thread SyncThread;

    //Store the incoming messages that we need to deal with
    static List<string> incomingMessages = new List<string>();
    static List<string> beingRead = new List<string>();

    // Start is called before the first frame update
    public static void Init()
    {
        SyncThread = new Thread(ThreadedSync);
        SyncThread.Start();
        incomingMessages = new List<string>();
        active = true;
    }

    public static void HaltImmediately()
    {
        Debug.Log("Halt");
        SyncThread.Abort();
        syncTimer.Close();
    }

    public static void ReceiveMessage(string message)
    {
        if(active)
        {
            incomingMessages.Add(message);
        }
    }

    //This function contains the code to allow another thread to handle networking
    public static void ThreadedSync()
    {
        // Create a timer with a two second interval.
        syncTimer = new System.Timers.Timer(20);
        // Hook up the Elapsed event for the timer. 
        syncTimer.Elapsed += OnSync;
        syncTimer.AutoReset = true;
        syncTimer.Enabled = true;
    }

    //Pulled from C# documentation
    private static void OnSync(System.Object source, ElapsedEventArgs e)
    {
        SendPosition();
        ReadMessages();
    }

    static void SendPosition()
    {
        //Sends the positions of all of the player's objects to the server
        var keys = playerManager.ourPlayers.Keys.ToArray();

        string message = "0|" + netComs.socketId + "!";
        long currentTime = netComs.GetTime();

        foreach (int key in keys)
        {
            if(playerManager.ourPlayers.ContainsKey(key))
            {
                Player player = playerManager.ourPlayers[key];

                message += player.Id + ":";
                message += currentTime + ":";
                message += player.XPos + ":";
                message += player.YPos + ":";
                message += player.Size + "?";

            }
        }

        

        message = message.Trim('?');

        //Debug.Log("sending " + message);

        //Send the message to the server
        netComs.NBSendMessage(2, message);
    }

    static void ReadMessages()
    {
        beingRead = incomingMessages;
        incomingMessages = new List<string>();

        foreach (string message in beingRead)
        {
            //Debug.Log("reading " + message);

            string[] getId = message.Split('!');

            int clientId = int.Parse(getId[0]);

            string remainingMessage = getId[1];

            string[] data = remainingMessage.Split('?');

            Dictionary<int, PlayerData> incomingPlayers = new Dictionary<int, PlayerData>();

            foreach (string player in data)
            {
                string[] vals = player.Split(':');

                int playerId = int.Parse(vals[0]);
                long time = long.Parse(vals[1]);
                float x = float.Parse(vals[2]);
                float y = float.Parse(vals[3]);
                float z = 0f;
                int size = int.Parse(vals[4]);
                string clientName = vals[5];
                float clientRed = float.Parse(vals[6]);
                float clientGreen = float.Parse(vals[7]);

                incomingPlayers.Add(playerId, new PlayerData(playerId, clientId, time, size, x, y, z, clientName, clientRed, clientGreen));
            }

            int[] addPlayers;
            int[] removePlayers;


            if (objectManager.currentClients.ContainsKey(clientId))
            {
                var currentPlayersIds = objectManager.currentClients[clientId].Players.Keys.ToArray();
                var incomingPlayersIds = incomingPlayers.Keys;

                addPlayers = incomingPlayersIds.Except(currentPlayersIds).ToArray();
                removePlayers = currentPlayersIds.Except(incomingPlayersIds).ToArray();
            }
            else
            {
                addPlayers = incomingPlayers.Keys.ToArray(); ;
                removePlayers = new int[0];
            }

            foreach (int Id in addPlayers)
            {
                objectManager.addPlayers.Add(incomingPlayers[Id]);
            }

            foreach (int Id in removePlayers)
            {
                Debug.Log("removing player");
                objectManager.removePlayers.Add(new IdPair(clientId, Id));
            }

            foreach(int Id in incomingPlayers.Keys)
            {
                if (objectManager.currentClients.ContainsKey(incomingPlayers[Id].ClientId))
                {
                    ClientData client = objectManager.currentClients[incomingPlayers[Id].ClientId];

                    if(client.Players.ContainsKey(incomingPlayers[Id].Id))
                    {
                        client.Players[incomingPlayers[Id].Id].UpdateData(incomingPlayers[Id].Time, incomingPlayers[Id].Size, incomingPlayers[Id].XPos, incomingPlayers[Id].YPos, incomingPlayers[Id].ZPos);
                    }
                }
            }
        }
    }
}
