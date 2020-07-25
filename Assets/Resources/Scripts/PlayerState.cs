using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public BaseHero Hero0;
    public BaseHero Hero1;
    public BaseHero Hero2;
    public BaseHero Hero3;
    public int Level;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Hero0 = this.transform.Find("Hero0").GetComponent<BaseHero>();
        Hero1 = this.transform.Find("Hero1").GetComponent<BaseHero>();
        Hero2 = this.transform.Find("Hero2").GetComponent<BaseHero>();
        Hero3 = this.transform.Find("Hero3").GetComponent<BaseHero>();
    }
}
