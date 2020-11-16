using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class loadGame : MonoBehaviour
{
    static string IP;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        PlayerMovement.keyboardEnable = GameObject.Find("KeyboardToggle").GetComponent<Toggle>().isOn;
        netComs.IPAddress = IP;
        SceneManager.LoadScene("game");
    }

    public void endEdit()
    {
        IP = GameObject.Find("IP").GetComponent<InputField>().text;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
