using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Timers;
using System.Threading;
using System;

public class orbSync
{
    static bool active = false;
    static System.Timers.Timer syncTimer;
    static Thread SyncThread;

    //Store the incoming messages that we need to deal with
    static List<String> incomingOrbMessages = new List<string>();
    static List<String> beingReadOrb = new List<string>();

    //Store the incoming messages that we need to deal with
    static List<String> incomingSyncMessages = new List<string>();
    static List<String> beingReadSync = new List<string>();

    // Start is called before the first frame update
    public static void Init()
    {
        SyncThread = new Thread(ThreadedSync);
        SyncThread.Start();
        incomingOrbMessages = new List<string>();
        incomingSyncMessages = new List<string>();
        active = true;
    }

    public static void HaltImmediately()
    {
        Debug.Log("Halt");
        SyncThread.Abort();
        syncTimer.Close();
    }

    public static void ReceiveOrbMessage(string message)
    {
        if (active)
        {
            incomingOrbMessages.Add(message);
        }
    }

    public static void ReceiveSyncMessage(string message)
    {
        if (active)
        {
            incomingSyncMessages.Add(message);
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
        ReadMessagesOrb();
    }

    /*static void SendPosition()
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
    }*/

    static void ReadMessagesOrb()
    {
        beingReadOrb = incomingOrbMessages;
        incomingOrbMessages = new List<string>();
        Debug.Log(beingReadOrb.Count);
        foreach (string message in beingReadOrb)
        {
            string[] orbs = message.Split('?');

            foreach(string orb in orbs)
            {
                string[] data = orb.Split(':');

                float XPos = float.Parse(data[0]) * manager.Width - (0.5f * manager.Width);
                float YPos = float.Parse(data[1]) * manager.Height- (0.5f * manager.Height);
                long Id = long.Parse(data[2]);

                objectManager.addOrbs.Add(new OrbData(Id, XPos, YPos));
            }
        }
    }
}



