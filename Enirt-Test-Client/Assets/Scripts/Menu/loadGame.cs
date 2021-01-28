using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class loadGame : MonoBehaviour
{
    static string IP;
    public Slider colorSlider;
    public static string errorText = "";

    // Start is called before the first frame update
    void Start()
    {
        colorValueChange();
        GameObject.Find("Error").GetComponent<UnityEngine.UI.Text>().text = errorText;
        colorSlider.onValueChanged.AddListener(delegate { colorValueChange(); });
    }

    public void colorValueChange()
    {
        float sliderValue = colorSlider.value;
        float red = 1.0f - sliderValue;
        float green = 0.0f + sliderValue;
        manager.clientColor = new Color(red, green, 0.0f);

        colorSlider.gameObject.transform.Find("Background").GetComponent<Image>().color = manager.clientColor;
        colorSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = manager.clientColor;
    }


    public void StartGame()
    {
        //Set necessary parameters and opens the game scene
        PlayerMovement.keyboardEnable = GameObject.Find("KeyboardToggle").GetComponent<Toggle>().isOn;
        manager.clientName = GameObject.Find("CharacterName").GetComponent<InputField>().text;

        netComs.IPAddress = IP;
        SceneManager.LoadScene("game");
    }

    public void endEdit()
    {
        //Updates the IP address value
        IP = GameObject.Find("IP").GetComponent<InputField>().text;
    }

    public void QuitGame()
    {
        //Quits the game
        Application.Quit();
    }
}
