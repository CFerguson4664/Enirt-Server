using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class scoreboard : MonoBehaviour
{

    public static GameObject playerScorePrefabStatic;
    public static GameObject uiTransformStatic;
    public static Vector3 scoreboardLocationStatic;
    public static GameObject scoreboardLabelStatic;

    public GameObject playerScorePrefab;
    public GameObject uiTransform;
    public GameObject scoreboardLabel;

    private static List<GameObject> playerScores = new List<GameObject>();

    void Start()
    {
        playerScorePrefabStatic = playerScorePrefab;
        uiTransformStatic = uiTransform;
        scoreboardLabelStatic = scoreboardLabel;
        scoreboardLocationStatic = scoreboardLabelStatic.transform.position;
        scoreboardLocationStatic.x += 100;

        // add client to scoreboard
        playerScorePrefabStatic.GetComponent<scoreComponent>().playerName.text = playerManager.ourPlayers[0].name;
        playerScorePrefabStatic.GetComponent<scoreComponent>().playerScore.text = playerManager.ourPlayers[0].Size.ToString();

        scoreboardLocationStatic.y -= 78;
        playerScores.Add(Instantiate(playerScorePrefabStatic, scoreboardLocationStatic, Quaternion.identity, uiTransformStatic.transform));

        // test score player
        // addScoreboardPlayer(playerManager.ourPlayers[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(playerScores.Count == 0)
        {
            return;
        }

        // updates client player
        int score = 0;
        foreach (KeyValuePair<int, Player> p in playerManager.ourPlayers)
        {
            try {
                playerScores[0].GetComponent<scoreComponent>().playerName.text = p.Value.name;
                score += p.Value.Size;
            }
            catch
            {
                removeScoreboardPlayer(0);
            }
        }
        playerScores[0].GetComponent<scoreComponent>().playerScore.text = score.ToString();

        // updates enemy players
        int i = 1;
        foreach(ClientData data in objectManager.currentClients.Values)
        {
            score = 0;
            foreach (KeyValuePair<int, PlayerData> p in data.Players)
            {
                try
                {
                    playerScores[0].GetComponent<scoreComponent>().playerName.text = p.Value.name;
                    score += p.Value.Size;
                }
                catch
                {
                    removeScoreboardPlayer(i);
                }
            }
            playerScores[i].GetComponent<scoreComponent>().playerScore.text = score.ToString();
            i++;
        }

    }

    public static void addScoreboardPlayer(PlayerData newPlayer)
    {
        Debug.Log("adding scoreboard...");
        try
        {
            playerScorePrefabStatic.GetComponent<scoreComponent>().playerName.text = newPlayer.name;
            
            int score = 0;
            foreach (KeyValuePair<int, PlayerData> p in objectManager.currentClients[newPlayer.ClientId].Players)
            {
                try
                {
                    score += p.Value.Size;
                }
                catch
                {
                    //removeScoreboardPlayer(i);
                }
            }
            playerScorePrefabStatic.GetComponent<scoreComponent>().playerScore.text = score.ToString();
            playerScorePrefabStatic.GetComponent<scoreComponent>().playerName.text = newPlayer.name;

            scoreboardLocationStatic.y -= 78;
            playerScores.Add(Instantiate(playerScorePrefabStatic,scoreboardLocationStatic, Quaternion.identity, uiTransformStatic.transform));
        }
        catch
        {
            return;
        }
    }

    public static void removeScoreboardPlayer(int i)
    {
        playerScores.RemoveAt(i);
        scoreboardLocationStatic.y += 78;
    }

}