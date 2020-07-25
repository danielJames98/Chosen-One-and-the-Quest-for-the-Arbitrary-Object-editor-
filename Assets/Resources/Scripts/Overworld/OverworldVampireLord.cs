using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldVampireLord : OverworldBaseEnemy
{
    bool doorsLocked;
    public GameObject door1;
    public GameObject door2;

    new public void Awake()
    {
        //initialisation
        Enemy0 = this.transform.Find("Enemy0").GetComponent<BaseEnemy>();
        Enemy1 = this.transform.Find("Enemy1").GetComponent<BaseEnemy>();
        Enemy2 = this.transform.Find("Enemy2").GetComponent<BaseEnemy>();
        Enemy3 = this.transform.Find("Enemy3").GetComponent<BaseEnemy>();
        Animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        mySprite = GetComponent<SpriteRenderer>();

        enemyState = GameObject.Find("EnemyState").GetComponent<EnemyState>();

        Sensor = gameObject.transform.Find("Sensor").gameObject;
        sensorCollider = Sensor.GetComponent<BoxCollider2D>();
        Sensor.GetComponent<Sensor>().FindStuff(this.gameObject, this.gameObject.GetComponent<OverworldBaseEnemy>());

        rangeSensor = gameObject.transform.Find("rangeSensor").gameObject;
        rangeSensorCollider = rangeSensor.GetComponent<BoxCollider2D>();
        rangeSensor.GetComponent<rangeSensor>().FindStuff(this.gameObject, this.gameObject.GetComponent<OverworldBaseEnemy>());

        if (gameObject.transform.Find("enemyAttackHitbox") != null)
        {
            attackHitbox = gameObject.transform.Find("enemyAttackHitbox").gameObject;
            attackHitbox.GetComponent<enemyAttackHitbox>().FindStuff(this.gameObject, this.gameObject.GetComponent<OverworldBaseEnemy>());
            attackHitbox.SetActive(false);

        }

        chaseSensor = gameObject.transform.Find("chaseSensor").gameObject;
        chaseSensor.GetComponent<chaseSensor>().FindStuff(this.gameObject, this.gameObject.GetComponent<OverworldBaseEnemy>());

        groundedCheck = gameObject.transform.Find("groundCheck").gameObject.transform;
        canAttack = true;

        LevelCheck();

        door1 = GameObject.Find("BossDoor1");
        door2 = GameObject.Find("BossDoor2");
        doorsLocked = false;
    }

    public void Update()
    {
        if (Hero==null && doorsLocked == false)
        {            
            door1.SetActive(false);
            door2.SetActive(false);
        }
        else if(Hero!=null &&doorsLocked==false)
        {
            door1.SetActive(true);
            door2.SetActive(true);
            doorsLocked = true;
        }
    }
}
