using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menuMusic;

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" && audioSource.clip == null && audioSource!=null)
        {
            audioSource.clip = menuMusic;
            audioSource.Play();
        }

        if(SceneManager.GetActiveScene().name!="MainMenu" && SceneManager.GetActiveScene().name!="ControllsMenu" && audioSource!=null)
        {
            audioSource.clip = null;
        }
    }
    public void Awake()
    {
        if(this.gameObject.name != "QuestCompleteButton" && this.gameObject.name!="QuestCompleteButton(Clone)")
        {
            DontDestroyOnLoad(this);
            audioSource = this.GetComponent<AudioSource>();
        }
        

    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void MainMenuFromGame()
    {
        GameObject.Find("WorldState").GetComponent<WorldState>().gameComplete = false;
        Object.Destroy(GameObject.Find("QuestCompleteCanvas(Clone)"));
        Object.Destroy(GameObject.Find("WorldState"));
        SceneManager.LoadScene("MainMenu");       
    }

    public void NewGame()
    {       
        SceneManager.LoadScene("Plains");       
    }

    public void ControllsMenu()
    {
        SceneManager.LoadScene("ControllsMenu");
    }

    public void quit()
    {
        Application.Quit();
    }
}