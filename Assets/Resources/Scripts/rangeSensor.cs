﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rangeSensor : MonoBehaviour
{
    public GameObject owner;
    public OverworldBaseEnemy ownerScript;
    public BoxCollider2D boxCollider2D;

    public void FindStuff(GameObject ownerRef, OverworldBaseEnemy ownerScriptRef)
    {
        owner = ownerRef;
        ownerScript = ownerScriptRef;
        boxCollider2D = this.GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if(this.gameObject.transform.position != owner.transform.position)
        {
            this.gameObject.transform.position = owner.transform.position;
        }

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Hero")
        {
            ownerScript.heroInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Hero")
        {
            ownerScript.heroInRange = false;
        }
    }
}
