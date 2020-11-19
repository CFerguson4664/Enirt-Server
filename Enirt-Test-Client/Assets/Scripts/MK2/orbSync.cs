using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Timers;
using System.Threading;
using System;
using System.Linq;

public class OrbSync
{
    static bool active = false;
    static System.Timers.Timer syncTimer;
    static Thread SyncThread;

    //Store the incoming messages that we need to deal with
    static List<string> incomingOrbMessages = new List<string>();
    static List<string> beingReadOrb = new List<string>();

    //Store the incoming messages that we need to deal with
    static List<string> incomingSyncMessages = new List<string>();
    static List<string> beingReadSync = new List<string>();

    //Store the incoming messages that we need to deal with
    static List<string> incomingSyncRequestMessages = new List<string>();
    static List<string> beingReadSyncRequest = new List<string>();

    public static void ResetFile()
    {
        //Store the incoming messages that we need to deal with
        incomingOrbMessages = new List<string>();
        beingReadOrb = new List<string>();

        //Store the incoming messages that we need to deal with
        incomingSyncMessages = new List<string>();
        beingReadSync = new List<string>();

        //Store the incoming messages that we need to deal with
        incomingSyncRequestMessages = new List<string>();
        beingReadSyncRequest = new List<string>();
    }

    // Start is called before the first frame update
    public static void Init()
    {
        ResetFile();
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

    public static void ReceiveSyncRequestMessage(string message)
    {
        if (active)
        {
            incomingSyncRequestMessages.Add(message);
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
        ReadMessagesOrb();
        ReadMessagesSync();
        ReadMessagesSyncRequest();
    }

    static void ReadMessagesSyncRequest()
    {
        beingReadSyncRequest = incomingSyncRequestMessages;
        incomingSyncRequestMessages = new List<string>();

        foreach (string request in beingReadSyncRequest)
        {
            Debug.Log("Generating Sync Data");

            //Create the message string
            string message = "3|";

            var currentIds = ObjectManager.currentOrbs.Keys.ToArray(); //To array is used in case the source needs to be modified
            int counter = 0;

            foreach (long Id in currentIds)
            {
                //Limit the number of orbs in game to 1000, because the message size otherwise might get huge; also make sure the message fits in our buffer
                if(counter >= 5000 || message.Length > 149950)
                {
                    break;
                }

                OrbData current = ObjectManager.currentOrbs[Id];
                message += current.XPos + ":";
                message += current.YPos + ":";
                message += current.Id + "?";
                counter++;
            }

            //Remove the trailing '?'
            message = message.Trim('?');


            //Send the message to the server
            if(counter >= 5000 || message.Length > 149950)
            {
                NetComs.NBSendMessage(50, message);
            }
            else
            {
                NetComs.NBSendMessage(2, message);
            }
        }
    }

    static void ReadMessagesSync() 
    {
        
        beingReadSync = incomingSyncMessages;
        incomingSyncMessages = new List<string>();

        Dictionary<long, OrbData> incomingData = new Dictionary<long, OrbData>();

        foreach (string message in beingReadSync)
        {
            string[] orbs = message.Split('?');

            foreach (string orb in orbs)
            {
                string[] data = orb.Split(':');

                float XPos = float.Parse(data[0]);
                float YPos = float.Parse(data[1]);
                long Id = long.Parse(data[2]);

                incomingData.Add(Id,new OrbData(Id, XPos, YPos));
            }

            //These create IEnumerable objects that can then be converted into arrays. 
            var currentIds = ObjectManager.currentOrbs.Keys.ToArray(); //To array is used in case the source needs to be modified
            var incomingIds = incomingData.Keys; //To array is not used here because the source will not be modified

            //Removes all items in the second array from the first array
            var toAdd = incomingIds.Except(currentIds).ToArray();
            var toRemove = currentIds.Except(incomingIds).ToArray();

            foreach (long Id in toAdd)
            {
                ObjectManager.addOrbs.Add(incomingData[Id]);
            }

            //Append all of the ids in to remove to the list in object manager
            ObjectManager.removeOrbs.Concat(toRemove);
        }
    }
    

    static void ReadMessagesOrb()
    {
        beingReadOrb = incomingOrbMessages;
        incomingOrbMessages = new List<string>();
        foreach (string message in beingReadOrb)
        {
            
            string[] orbs = message.Split('?');

            foreach(string orb in orbs)
            {
                string[] data = orb.Split(':');

                float XPos = (float)Math.Round(float.Parse(data[0]) * Manager.Width - (0.5f * Manager.Width),2);
                float YPos = (float)Math.Round(float.Parse(data[1]) * Manager.Height- (0.5f * Manager.Height),2);
                long Id = long.Parse(data[2]);


                //Debug.Log("Orb at (" + XPos + "," + YPos + ") and time " + Id);

                ObjectManager.addOrbs.Add(new OrbData(Id, XPos, YPos));
            }
        }
    }
}



