using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class objectManager : MonoBehaviour
{
    //The template object used to represent other players
    public static GameObject marker;
    public GameObject markerObj;

    public static GameObject orb;
    public GameObject OrbObj;

    //Store the current data for each player in a dictionary indexed with the player ids.
    //The data in this is updated by playerSync.
    public static Dictionary<int,PlayerData> players = new Dictionary<int,PlayerData>();


    //Stores the orbs currently being displayed
    public static Dictionary<long, OrbData> currentOrbs = new Dictionary<long, OrbData>();

    //Stores the ids of the orbs to be removed the next cycle
    public static List<OrbData> addOrbs = new List<OrbData>();
    //Stores the ids of the orbs to be added the next cycle
    public static List<long> removeOrbs = new List<long>();

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
            InterpolatePlayer(player);
        }

        AddOrbs();
        RemoveOrbs();
    }

    static void AddOrbs()
    {
        long time = netComs.GetTime();
        foreach (OrbData orbData in addOrbs)
        {
            if(orbData.Id < time)
            {
                if (!currentOrbs.ContainsKey(orbData.Id))
                {
                    orbData.gameObject = Instantiate(orb, new Vector3(orbData.XPos, orbData.YPos, orbData.ZPos), Quaternion.identity);
                    currentOrbs.Add(orbData.Id, orbData);
                }
            }
        }
    }

    static void RemoveOrbs()
    {
        foreach (long Id in removeOrbs)
        {
            if (currentOrbs.ContainsKey(Id))
            {
                if(currentOrbs[Id].gameObject != null) {
                    Destroy(currentOrbs[Id].gameObject);
                }
                currentOrbs.Remove(Id);
            }
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
