using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
 

    public void OnHostGameClick()
    {
        Debug.Log("Host game clicked!");
        SceneManager.LoadScene("SampleScene");
    }

    public void OnJoinGameClick()
    {
        Debug.Log("Join game clicked!");
    }

    public void OnQuitGameClick()
    {
        Debug.Log("Quit clicked!");
        Application.Quit();
    }
}
