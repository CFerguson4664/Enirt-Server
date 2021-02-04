using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class scoreboard : MonoBehaviour
{
    public static bool scoreboardActive = false;

    public static GameObject playerScorePrefabStatic;
    public static Vector3 scoreboardLocationStatic;
    public static GameObject scoreboardLabelStatic;
    public static GameObject CanvasStatic;

    public GameObject playerScorePrefab;
    public GameObject scoreboardLabel;
    public GameObject Canvas;


    public static List<ScoreData> scoreDatas = new List<ScoreData>();
    private static List<Score> playerScores = new List<Score>();

    void Start()
    {
        CanvasStatic = Canvas;
        playerScorePrefabStatic = playerScorePrefab;
        scoreboardLabelStatic = scoreboardLabel;
        scoreboardLocationStatic = scoreboardLabelStatic.transform.position;
        scoreboardLocationStatic.x += 100;
        scoreboardLocationStatic.y -= 79;
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboardLabelStatic.SetActive(true);
            foreach (Score s in playerScores)
            {
                s.textBox.SetActive(true);
            }
        }

        if(Input.GetKey(KeyCode.Tab))
        {
            GetScoreData();
            DisplayScores();
        }

        if(Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboardLabelStatic.SetActive(false);
            foreach (Score s in playerScores)
            {
                s.textBox.SetActive(false);
            }
        }
    }

    public static void DisplayScores()
    {
        List<ScoreData> sortedScores = scoreDatas.OrderByDescending(score => score.score).ToList();

        for(int i = 0; i < sortedScores.Count; i++)
        {
            if(playerScores.Count > i)
            {
                playerScores[0].UpdateScore(sortedScores[i]);
            }
            else if(playerScores.Count < 10)
            {
                playerScores.Add(new Score(playerScorePrefabStatic, CanvasStatic, scoreboardLocationStatic, sortedScores[i], (i + 1)));
                scoreboardLocationStatic.y -= 79;
            }
            else
            {
                break;
            }
        }

        if(playerScores.Count > sortedScores.Count)
        {
            for(int j = sortedScores.Count; j < playerScores.Count; j++)
            {
                Destroy(playerScores[j].textBox);
                playerScores.RemoveAt(j);
                scoreboardLocationStatic.y += 79;
            }
        }
    }

    public static void GetScoreData()
    {
        scoreDatas = new List<ScoreData>();

        int score = 0;
        string name = "";
        Color color = Color.black;

        foreach (Player player in playerManager.ourPlayers.Values)
        {
            score += player.Size;
            name = player.name;
            color = player.color;
        }
        scoreDatas.Add(new ScoreData(score, name, color));

        foreach (ClientData data in objectManager.currentClients.Values)
        {
            score = 0;
            foreach (PlayerData playerData in data.Players.Values)
            {
                score += playerData.Size;
                name = playerData.name;
                color = playerData.color;
            }
            scoreDatas.Add(new ScoreData(score, name, color));
        }
    }
}


public class ScoreData 
{
    public int score;
    public string name;
    public Color color;

    public ScoreData(int score, string name, Color color)
    {
        this.score = score;
        this.name = name;
        this.color = color;
    }
}


public class Score
{
    public int score;
    public string name;
    public Color color;
    public int position;

    public GameObject textBox;

    public Score(GameObject prefab, GameObject canvas, Vector3 transform, ScoreData data, int position)
    {
        textBox = GameObject.Instantiate(prefab, transform, Quaternion.identity, canvas.transform);
        SetName(data.name);
        SetScore(data.score);
        SetColor(data.color);
        SetPosition(position);
    }

    public Score(GameObject prefab, GameObject canvas, Vector3 transform) 
    {
        textBox = GameObject.Instantiate(prefab, transform, Quaternion.identity, canvas.transform);
    }

    public Score(GameObject prefab, GameObject canvas, Vector3 transform, string name, Color color, int score, int position)
    {
        textBox = GameObject.Instantiate(prefab, transform, Quaternion.identity, canvas.transform);
        SetName(name);
        SetScore(score);
        SetColor(color);
        SetPosition(position);
    }

    public void UpdateScore(ScoreData data)
    {
        SetName(data.name);
        SetColor(data.color);
        SetScore(data.score);
    }

    public void SetName(string name)
    {
        this.name = name;
        textBox.GetComponent<scoreComponent>().playerName.text = position.ToString() + ". " + name;
    }

    public void SetScore(int score)
    {
        this.score = score;
        textBox.GetComponent<scoreComponent>().playerScore.text = score.ToString();
    }

    public void SetColor(Color color)
    {
        this.color = color;
        textBox.GetComponent<scoreComponent>().playerName.color = color;
        textBox.GetComponent<scoreComponent>().playerScore.color = color;
    }

    public void SetPosition(int position)
    {
        this.position = position;
    }

    public void SetPosition(Vector3 pos)
    {
        textBox.transform.position = pos;
    }
}