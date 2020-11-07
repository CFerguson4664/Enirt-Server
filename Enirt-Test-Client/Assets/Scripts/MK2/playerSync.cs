using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Timers;
using System.Threading;
using System;

public class playerSync
{
    static bool active = false;
    static System.Timers.Timer syncTimer;
    static Thread SyncThread;

    //Store the incoming messages that we need to deal with
    static List<String> incomingMessages = new List<string>();
    static List<String> beingRead = new List<string>();

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
        Debug.Log("On sync");
        SendPosition();
        ReadMessages();
    }

    static void SendPosition()
    {
        try
        {
            Vector3 playerPos = PlayerMovement.currentPostiton;

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
        }
    }

    static void ReadMessages()
    {
        beingRead = incomingMessages;
        incomingMessages = new List<string>();
        try
        {
            Debug.Log(beingRead.Count);
            foreach (string message in beingRead)
            {
                string[] vals = message.Split(':');

                Debug.Log("Message: " + message);
                Debug.Log("Vals 0: " + vals[0]);

                int playerId = int.Parse(vals[0]);
                long time = long.Parse(vals[1]);
                float x = float.Parse(vals[2]);
                float y = float.Parse(vals[3]);
                float z = 0f;
                int size = int.Parse(vals[4]);


                if(objectManager.players.ContainsKey(playerId))
                {
                    Debug.Log("Updating data");
                    PlayerData currentData = objectManager.players[playerId];

                    currentData.UpdateData(time, size, x, y, z);
                }
                else
                {
                    Debug.Log("Creating new data");
                    objectManager.players.Add(playerId, new PlayerData(playerId, time, size, x, y, z));
                }
            }
        } 
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }
    }
}
