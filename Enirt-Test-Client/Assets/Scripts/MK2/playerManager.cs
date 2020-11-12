using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public static Dictionary<int, PlayerData> ourPlayers = new Dictionary<int, PlayerData>();
    public  static GameObject player;
    public  GameObject playerObj;

    public static void AddPlayer(PlayerData playerData, bool glide)
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

        playerData.gameObject = Instantiate(player, new Vector3(playerData.XPos, playerData.YPos, playerData.ZPos), Quaternion.identity);
        playerData.gameObject.GetComponent<eatDots>().size = playerData.Size;
        playerData.Id = playerId;

        if(glide)
        {
            playerData.gameObject.GetComponent<PlayerMovement>().StartGlide();
        }

        ourPlayers.Add(playerId, playerData);
    }

    static void RemovePlayer(int Id)
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
        player = playerObj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
