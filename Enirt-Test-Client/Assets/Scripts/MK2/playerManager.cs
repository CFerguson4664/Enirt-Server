using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class playerManager : MonoBehaviour
{
    public static float cameraSpeed = 10;
    public static float cameraSlack = 2;
    public static float cameraZoomSpeed = 0.5f;

    public static Dictionary<int, Player> ourPlayers = new Dictionary<int, Player>();
    public  static GameObject playerPrefab;
    public  GameObject playerObj;

    public static void ResetFile()
    {
        ourPlayers = new Dictionary<int, Player>();
    }

    void Awake()
    {
        ResetFile();
    }

    public static void AddPlayer(PlayerData playerData, int size, bool glide)
    {
        int playerId = 0;
        for(int i = 0; i < int.MaxValue; i++)
        {
            if(!ourPlayers.ContainsKey(i))
            {
                playerId = i;
                break;
            }
        }

        Player player = new Player(playerData);

        player.gameObject = Instantiate(playerPrefab, new Vector3(player.XPos, player.YPos, player.ZPos), Quaternion.identity);
        player.gameObject.GetComponent<eatDots>().size = size;
        player.gameObject.GetComponent<PlayerMovement>().Id = playerId;
        player.SetRecombine();
        player.Id = playerId;

        if(glide)
        {
            player.gameObject.GetComponent<PlayerMovement>().StartGlide();
        }

        ourPlayers.Add(playerId, player);
    }

    public static void RemovePlayer(int Id)
    {
        if (ourPlayers.ContainsKey(Id))
        {
            if (ourPlayers[Id].gameObject != null)
            {
                Destroy(ourPlayers[Id].gameObject);
            }
            ourPlayers.Remove(Id);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        playerPrefab = playerObj;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckRecombines();
    }

    void Update()
    {
        MoveCamera();
    }

    void CheckRecombines()
    {
        var keys = ourPlayers.Keys.ToArray();

        foreach (int key in keys)
        {
            if (ourPlayers.ContainsKey(key))
            {
                Player player = ourPlayers[key];
                if (player.currentPlayerCollisions.Count > 0)
                {
                    foreach (int i in player.currentPlayerCollisions)
                    {
                        if(ourPlayers.ContainsKey(i))
                        {
                            Player otherPlayer = ourPlayers[i];

                            if(player.recombineTime <= 0 && otherPlayer.recombineTime <= 0)
                            {
                                player.gameObject.GetComponent<eatDots>().size += otherPlayer.gameObject.GetComponent<eatDots>().size;
                                player.SetRecombine();
                                RemovePlayer(otherPlayer.Id);
                            }
                        }
                    }
                }

                if(player.recombineTime > 0)
                {
                    player.recombineTime -= Time.fixedDeltaTime;
                    //Debug.Log(player.recombineTime);
                }
            }
        }
    }

    void MoveCamera()
    {
        var keys = ourPlayers.Keys.ToArray();

        float totalX = 0;
        float totalY = 0;
        float totalS = 0;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        float minY = float.MaxValue;
        float maxY = float.MinValue;


        foreach (int key in keys)
        {
            if(ourPlayers.ContainsKey(key))
            {
                Player player = ourPlayers[key];
                totalX += player.XPos * player.Size;
                totalY += player.YPos * player.Size;
                totalS += player.Size;

                float radius = Mathf.Sqrt(player.Size / 50f / Mathf.PI);

                if (player.XPos + radius > maxX)
                {
                    maxX = player.XPos + radius;
                }
                if(player.XPos - radius < minX)
                {
                    minX = player.XPos - radius;
                }
                if (player.YPos + radius> maxY)
                {
                    maxY = player.YPos + radius;
                }
                if (player.YPos - radius < minY)
                {
                    minY = player.YPos - radius;
                }
            }
        }
        float wAvgX = totalX / totalS;
        float wAvgY = totalY / totalS;


        float cameraViewHeight = Camera.main.orthographicSize * 2;
        float cameraViewWidth = cameraViewHeight * Camera.main.aspect;

        if (keys.Length == 0)
        {
            wAvgX = 0;
            wAvgY = 0;
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;
        }

        float offsetX = maxX - minX;
        float offsetY = maxY - minY;

        if(offsetX >= cameraViewWidth * 0.6 || offsetY >= cameraViewHeight * 0.6)
        {
            Camera.main.orthographicSize += cameraZoomSpeed * Time.deltaTime;
        }
        else
        {
            if(Camera.main.orthographicSize > 5)
            {
                Camera.main.orthographicSize -= cameraZoomSpeed * Time.deltaTime;
            }
        }

        Vector3 cameraGoalPos = new Vector3(wAvgX, wAvgY, Camera.main.transform.position.z);

        float cameraSpeedAdj = Vector2.Distance(Camera.main.transform.position, cameraGoalPos) * cameraSpeed * (1 / cameraSlack) * Time.deltaTime;

        Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, cameraGoalPos, cameraSpeedAdj);
    }
}

public class Player : PlayerData
{
    public List<int> currentPlayerCollisions = new List<int>();

    public double recombineTime = 1;

    public void SetRecombine()
    {
        recombineTime = (Math.Pow(Math.Log(Size * 1d + 2), 1.7) * 3);
    }

    public Player(int sizeIn, float xIn, float yIn, float zIn) : base(sizeIn, xIn, yIn, zIn)
    {

    }

    public Player(PlayerData data) : base(data.Size, data.XPos, data.YPos, data.ZPos)
    {

    }
}
