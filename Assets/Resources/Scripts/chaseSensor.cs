using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chaseSensor : MonoBehaviour
{
    public GameObject owner;
    public OverworldBaseEnemy ownerScript;
    public BoxCollider2D boxCollider2D;
    public Sensor mainSensor;
    public GameObject hero;

    public void FindStuff(GameObject ownerRef, OverworldBaseEnemy ownerScriptRef)
    {
        owner = ownerRef;
        ownerScript = ownerScriptRef;
        boxCollider2D = this.GetComponent<BoxCollider2D>();
        mainSensor = ownerScript.Sensor.GetComponent<Sensor>();
        mainSensor.chaseSensor = this;
    }


    private void FixedUpdate()
    {
        if (this.gameObject.transform.position != owner.transform.position)
        {
            this.gameObject.transform.position = owner.transform.position;
        }

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == hero)
        {
            ownerScript.StopChase();
        }
    }
}
