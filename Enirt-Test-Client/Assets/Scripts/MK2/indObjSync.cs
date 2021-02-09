using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Threading;
using System;
using System.Linq;

public class indObjSync
{
    static bool active = false;
    static System.Timers.Timer syncTimer;
    static Thread SyncThread;

    //Store the incoming messages that we need to deal with
    static List<string> incomingGenMessages = new List<string>();
    static List<string> beingReadGen = new List<string>();

    //Store the incoming messages that we need to deal with
    static List<string> incomingSyncMessages = new List<string>();
    static List<string> beingReadSync = new List<string>();

    //Store the incoming messages that we need to deal with
    static List<string> incomingSyncRequestMessages = new List<string>();
    static List<string> beingReadSyncRequest = new List<string>();

    public static void ResetFile()
    {
        //Store the incoming messages that we need to deal with
        incomingGenMessages = new List<string>();
        beingReadGen = new List<string>();

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
        incomingGenMessages = new List<string>();
        incomingSyncMessages = new List<string>();
        active = true;
    }

    public static void HaltImmediately()
    {
        Debug.Log("Halt");
        SyncThread.Abort();
        syncTimer.Close();
    }

    public static void ReceiveGenMessage(string message)
    {
        if (active)
        {
            incomingGenMessages.Add(message);
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
        ReadMessagesGen();
        ReadMessagesSync();
        ReadMessagesSyncRequest();
    }

    static void ReadMessagesSyncRequest()
    {
        beingReadSyncRequest = incomingSyncRequestMessages;
        incomingSyncRequestMessages = new List<string>();

        foreach (string request in beingReadSyncRequest)
        {
            Debug.Log("Generating IndObj Sync Data");

            //Create the message string
            string message = "5|";

            var currentIds = objectManager.currentIndObj.Keys.ToArray(); //To array is used in case the source needs to be modified
            int counter = 0;

            foreach (long Id in currentIds)
            {
                //Limit the number of indObj in game to 1000, because the message size otherwise might get huge; also make sure the message fits in our buffer
                if (counter >= 5000 || message.Length > 149950)
                {
                    break;
                }

                IndObjData current = objectManager.currentIndObj[Id];
                message += current.XPos + ":";
                message += current.YPos + ":";
                message += current.Size + ":";
                message += current.Id + "?";
                counter++;
            }

            //Remove the trailing '?'
            message = message.Trim('?');


            //Send the message to the server
            if (counter >= 5000 || message.Length > 149950)
            {
                netComs.NBSendMessage(50, message);
            }
            else
            {
                netComs.NBSendMessage(2, message);
            }
        }
    }

    static void ReadMessagesSync()
    {

        beingReadSync = incomingSyncMessages;
        incomingSyncMessages = new List<string>();

        Dictionary<long, IndObjData> incomingData = new Dictionary<long, IndObjData>();

        foreach (string message in beingReadSync)
        {
            string[] indObjs = message.Split('?');

            foreach (string indObj in indObjs)
            {
                string[] data = indObj.Split(':');

                float XPos = float.Parse(data[0]);
                float YPos = float.Parse(data[1]);
                int Size = int.Parse(data[2]);
                long Id = long.Parse(data[3]);

                incomingData.Add(Id, new IndObjData(Id, XPos, YPos, Size));
            }

            //These create IEnumerable objects that can then be converted into arrays. 
            var currentIds = objectManager.currentIndObj.Keys.ToArray(); //To array is used in case the source needs to be modified
            var incomingIds = incomingData.Keys; //To array is not used here because the source will not be modified

            //Removes all items in the second array from the first array
            var toAdd = incomingIds.Except(currentIds).ToArray();
            var toRemove = currentIds.Except(incomingIds).ToArray();

            foreach (long Id in toAdd)
            {
                objectManager.addIndObj.Add(incomingData[Id]);
            }

            //Append all of the ids in to remove to the list in object manager
            objectManager.removeIndObj.Concat(toRemove);
        }
    }


    static void ReadMessagesGen()
    {
        beingReadGen = incomingGenMessages;
        incomingGenMessages = new List<string>();

        foreach (string message in beingReadGen)
        {

            int numToGen = int.Parse(message);
            Debug.Log("Gen " + numToGen + " IndObj");
            var rand = new System.Random();

            for (int i = 0; i < numToGen; i++)
            {
                double x = rand.NextDouble();
                double y = rand.NextDouble();
                long id = i;
                int Size = rand.Next(20, 2000);

                float XPos = (float)Math.Round(x * manager.Width - (0.5f * manager.Width), 2);
                float YPos = (float)Math.Round(y * manager.Height - (0.5f * manager.Height), 2);

                Debug.Log("IndObj at position (" + XPos + "," + YPos + ") and time " + id + " with size " + Size);

                objectManager.addIndObj.Add(new IndObjData(id, XPos, YPos, Size));
            }
        }
    }
}



