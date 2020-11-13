using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.Timeline;
using System.Timers;
using System.Threading;

public class netComs
{
    public bool enableNetworking;
    static bool enableNet;
    static NetworkStream networkStream;
    static TcpClient client;
    public static int socketId;
    static long clockOffset = 0;
    public static string IPAddress;

    static System.Timers.Timer networkTimer;
    static Thread NetworkThread;


    public void Init(bool enableNetworking)
    {
        if (enableNetworking)
        {
            Debug.Log(IPAddress);
            //Connect to the server
            networkStream = ConnectTCPClient(IPAddress, 8124);
            socketId = int.Parse(receiveMessages(networkStream)[0]);
            clockOffset = clockSync(networkStream);
            //Tell the server we are ready to begin receiving player data
            NBSendMessage(3, "0");
        }

        //Start the networking thread
        enableNet = enableNetworking;
        NetworkThread = new Thread(ThreadedSync);
        NetworkThread.Start();
    }

    public static void HaltImmediately()
    {
        if(enableNet)
        {
            //Halt the networking thread and close the connection to the server
            Debug.Log("Halt");
            NetworkThread.Abort();
            networkTimer.Close();
            networkStream.Close();
            client.Close();
        }
    }

    //This function contains the code to allow another thread to handle networking
    static void ThreadedSync()
    {
        // Create a timer with a two second interval.
        networkTimer = new System.Timers.Timer(20);
        // Hook up the Elapsed event for the timer. 
        networkTimer.Elapsed += OnSync;
        networkTimer.AutoReset = true;
        networkTimer.Enabled = true;
    }

    //Pulled from C# documentation, runs every 20ms
    static void OnSync(System.Object source, ElapsedEventArgs e)
    {
        //run only if networking is enabled
        if (enableNet)
        {
            //If there is data available
            if (networkStream.DataAvailable)
            {
                //receive all waiting messages
                string[] messages = receiveMessages(networkStream);

                //iterate through each message
                foreach (string singleMessage in messages)
                {
                    //If the messsage is not empty
                    if (singleMessage != "")
                    {
                        //process the command
                        string[] parts = singleMessage.Split('|');
                        if (parts[0] == "0")
                        {
                            playerSync.ReceiveMessage(parts[1]);
                        }
                        else if (parts[0] == "1")
                        {
                            orbSync.ReceiveOrbMessage(parts[1]);
                        }
                        else if (parts[0] == "2")
                        {
                            orbSync.ReceiveSyncRequestMessage(parts[1]);
                        }
                        else if (parts[0] == "3")
                        {
                            orbSync.ReceiveSyncMessage(parts[1]);
                        }
                    }
                }
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    NetworkStream ConnectTCPClient(String serverAddress, int port)
    {
        // Create a TcpClient.
        client = new TcpClient(serverAddress, port);

        // Get a client stream for reading and writing.
        NetworkStream stream = client.GetStream();

        return stream;
    }

    static void sendMessage(String message, NetworkStream stream)
    {
        // Translate the passed message into ASCII and store it as a Byte array.
        Byte[] data = System.Text.Encoding.ASCII.GetBytes(socketId + "," + message + "$");

        // Send the message to the connected TcpServer.
        stream.Write(data, 0, data.Length);
    }

    static String[] receiveMessages(NetworkStream stream)
    {
        // Buffer to store the response bytes.
        Byte[] data = new Byte[150000];

        // String to store the response ASCII representation.
        String[] responseData;

        // Read the first batch of the TcpServer response bytes.
        Int32 bytes = stream.Read(data, 0, data.Length);

        // Translate the passed message into a string
        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes).Split('$');

        return responseData;
    }

    public static void NBSendMessage(int command, String data)
    {
        sendMessage(command + "," + data, networkStream);
    }

    long clockSync(NetworkStream stream)
    {
        int numPings = 5;
        double sumOfOffsets = 0;

        for (int i = 0; i < numPings; i++)
        {
            long firstTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            sendMessage(1 + ",0", stream);
            String reply = receiveMessages(stream)[0];

            long secondTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            long serverTime = long.Parse(reply);

            long elapsedTime = secondTime - firstTime;

            //Latency in one direction
            long singleDirectionTime = elapsedTime / 2;

            //Number of seconds the server is ahead
            long aheadTime = (serverTime - firstTime) - singleDirectionTime;
            
            //Number of seconds the server is behind
            long behindTime = (secondTime - serverTime) - singleDirectionTime;

            //Average the amount the server is ahead and behind (invert behind so that it is the number of ms the server is ahead)
            //This offset can be added to the time on the client to convert it to the time on the server
            double offsetTime = (aheadTime + (-1 * behindTime)) / 2d;

            Debug.Log("Latency (single Direction): " + singleDirectionTime);
            Debug.Log("Offset time: " + offsetTime);

            sumOfOffsets += offsetTime;
        }

        long averageOffset = (long)(sumOfOffsets / numPings);

        Debug.Log("The average Offset is: " + averageOffset + " ms");

        return averageOffset;
    }

    public static long GetTime()
    {
        //Returns the time on the server in ms since epoc in UTC
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds + clockOffset;
    }
}
