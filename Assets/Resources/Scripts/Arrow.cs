using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject owner;
    public OverworldBanditArcher ownerScript;
    public BoxCollider2D boxCollider2D;
    public float velocity;

    public void FindStuff(GameObject ownerRef, OverworldBanditArcher ownerScriptRef)
    {
        owner = ownerRef;
        ownerScript = ownerScriptRef;
        boxCollider2D = this.GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Hero" && other.gameObject.GetComponent<OverworldPlayerController>().blocking==false)
        {
            ownerScript.heroHit = true;
            Destroy(this.gameObject);
        }
        else if ((other.gameObject.tag == "Hero" && other.gameObject.GetComponent<OverworldPlayerController>().blocking == true) || other.gameObject.tag == "Terrain")
        {
            Destroy(this.gameObject);
        }
    }
}