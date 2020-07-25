using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public GameObject owner;
    public OverworldBaseEnemy ownerScript;
    public BoxCollider2D boxCollider2D;
    public chaseSensor chaseSensor;

    public void FindStuff(GameObject ownerRef, OverworldBaseEnemy ownerScriptRef)
    {
        owner = ownerRef;
        ownerScript = ownerScriptRef;
        boxCollider2D = this.GetComponent<BoxCollider2D>();
    }


    private void FixedUpdate()
    {
        if (this.gameObject.transform.position != owner.transform.position)
        {
            this.gameObject.transform.position = owner.transform.position;
        }

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Hero" && ownerScript.heroSpotted==false)
        {           
            ownerScript.Hero = other.gameObject;
            ownerScript.HeroSpotted();
            chaseSensor.hero = other.gameObject;
        }       
    }
}
