using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManagerSpawner : MonoBehaviour
{
    public GameObject menuManager;

    private void Awake()
    {
        if(GameObject.Find("MenuManager")==null && GameObject.Find("MenuManager(Clone)") == null)
        {
            Instantiate(menuManager);
        }
    }
}
