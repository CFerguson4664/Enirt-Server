using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class playerSync : MonoBehaviour
{
    int counter = 0;
    public static GameObject marker;
    static Vector3 previous;
    static long previousTime = 0;
    static Vector3 current;
    static long currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        marker = GameObject.Find("Marker");
        current = marker.transform.position;
        previous = marker.transform.position;
        previousTime = netComs.GetTime();
        currentTime = netComs.GetTime() + 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(counter == 15)
        {
            counter = 0;
            sendPosition();
        }
        counter++;

        interpolateMarker();
    }

    void sendPosition()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        int size = marker.GetComponent<eatDots>().size;
        long currentTime = netComs.GetTime();

        string message = "0|" + netComs.socketId + ":" + currentTime + ":" + x + ":" + y + ":" + size;

        netComs.NBSendMessage(2, message);
    }

    public static void markerMove(string message)
    {
        string[] vals = message.Split(':');

        Debug.Log("Message: " + message);
        Debug.Log("Vals 0: " + vals[0]);

        long time = long.Parse(vals[1]);
        float x = float.Parse(vals[2]);
        float y = float.Parse(vals[3]);
        float z = marker.transform.position.z;
        int size = 50;//int.Parse(vals[4]);

        marker.GetComponent<eatDots>().size = size;

        previousTime = currentTime;
        currentTime = time;

        previous = current;
        current = new Vector3(x,y,z);
    }

    void interpolateMarker()
    {
        long time = netComs.GetTime();
        //Time since the position of the marker was last received
        long timeSinceData = time - currentTime;

        //Finding velocity of the player
        float xTravel = current.x - previous.x;
        float yTravel = current.y - previous.y;
        long timeTravel = currentTime - previousTime;
        float xVelocity = xTravel / timeTravel;
        float yVelocity = yTravel / timeTravel;

        float xPos = current.x + (xVelocity * timeSinceData);
        float yPos = current.y + (yVelocity * timeSinceData);
        float zPos = current.z;

        marker.transform.position = new Vector3(xPos, yPos, zPos);
    }
}
