using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    public string sceneToLoad;
    public WorldState worldState;
    public int playerXP;
    public int playerLevel;

    private void Awake()
    {
        worldState = GameObject.Find("WorldState").GetComponent<WorldState>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag=="Hero")
        {
            playerXP = collision.gameObject.GetComponent<OverworldPlayerController>().curXP;
            playerLevel = collision.gameObject.GetComponent<OverworldPlayerController>().curLevel;
            worldState.LoadLevel(sceneToLoad, playerXP, playerLevel);
        }
    }
}
