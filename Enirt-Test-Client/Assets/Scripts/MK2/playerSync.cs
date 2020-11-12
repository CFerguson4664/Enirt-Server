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
        /*try
        {
            Vector3 playerPos = PlayerMovement.currentPosition;

            //Get the position and size of the player object for this client
            float x = playerPos.x;
            float y = playerPos.y;
            int size = PlayerMovement.currentSize;
            long currentTime = netComs.GetTime();

            //Create the message string
            string message = "0|" + netComs.socketId + ":" + currentTime + ":" + x + ":" + y + ":" + size;

            //Send the message to the server
            netComs.NBSendMessage(2, message);
        } 
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }*/
    }

    static void ReadMessages()
    {
        beingRead = incomingMessages;
        incomingMessages = new List<string>();
        foreach (string message in beingRead)
        {
            string[] getId = message.Split('!');

            int clientId = int.Parse(getId[0]);

            string remainingMessage = getId[1];

            string[] data = remainingMessage.Split('?');

            Dictionary<int, PlayerData> incomingPlayers = new Dictionary<int, PlayerData>();

            foreach(string player in data)
            {
                string[] vals = player.Split(':');

                int playerId = int.Parse(vals[0]);
                long time = long.Parse(vals[1]);
                float x = float.Parse(vals[2]);
                float y = float.Parse(vals[3]);
                float z = 0f;
                int size = int.Parse(vals[4]);

                incomingPlayers.Add(playerId, new PlayerData(playerId, clientId, time, size, x, y, z));
            }

            var currentPlayersIds = objectManager.currentClients[clientId].Players.Keys.ToArray();
            var incomingPlayersIds = incomingPlayers.Keys;

            var addPlayers = incomingPlayersIds.Except(currentPlayersIds).ToArray();
            var removePlayers = currentPlayersIds.Except(incomingPlayersIds).ToArray();

            foreach(int Id in addPlayers)
            {
                objectManager.addPlayers.Add(incomingPlayers[Id]);
            }

            foreach(int Id in removePlayers)
            {
                objectManager.removePlayers.Add(new IdPair(clientId, Id));
            }
        }
    }
}
