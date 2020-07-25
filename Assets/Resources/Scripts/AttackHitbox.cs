using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public GameObject PlayerCharacter;
    public OverworldPlayerController overworldPlayerController;
    public BoxCollider2D boxCollider2D;

    public void FindStuff(GameObject pc, OverworldPlayerController pcScript)
    {
        PlayerCharacter = pc;

        overworldPlayerController = pcScript;
        boxCollider2D = this.GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            overworldPlayerController.enemyHit = true;
        }
    }

}