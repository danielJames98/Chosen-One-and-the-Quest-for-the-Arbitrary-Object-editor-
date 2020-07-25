using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldBanditArcher : OverworldBaseEnemy
{
    public GameObject projectile;
    public GameObject currentProjectile;
    public GameObject firePoint;

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

        firePoint = gameObject.transform.Find("FirePoint").gameObject;


        groundedCheck = gameObject.transform.Find("groundCheck").gameObject.transform;
        canAttack = true;
}

    new public void FixedUpdate()
    {
        //checks to see if player is on ground
        grounded = Physics2D.Linecast(transform.position, groundedCheck.position, 1 << LayerMask.NameToLayer("Level"));

        DirectionCheck();


        if (inCombat == true && heroInRange == false)
        {
            Chase();
        }

        if ((attacking == true) && ((Time.time - attackStart) > attackDuration))
        {
            StopAttack();
        }

        if ((Time.time - attackCooldownStart) > attackCooldown)
        {
            canAttack = true;
        }

        if (heroInRange == true && attacking == false && canAttack==true)
        {
            Attack();
        }

        if (attacking == true && (Time.time - attackStart) > attackHitTime && attackFired == false)
        {
            AttackHit();
        }

        if (heroHit == true)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().advantageState = "Enemy Advantage";
            SendParty();
        }

        if (attacking == true)
        {
            if (rb.velocity.x > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x - 0.1f, rb.velocity.y);
            }

            if (rb.velocity.x < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + 0.1f, rb.velocity.y);
            }
        }

        if ((rb.velocity.x<1 && rb.velocity.x>-1))
        {
            Idle();
        }

        if (jumping == true && grounded == true && ((Time.time - jumpStartTime) > 1))
        {
            jumping = false;
        }
    }

    new public void DirectionCheck()
    {
        if (Hero!=null)
        {
            if(leftFacingSprite==false)
            {
                if (horizInput < 0 || Hero.transform.position.x < this.gameObject.transform.position.x)
                {
                    mySprite.flipX = true;
                    firePoint.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                if (horizInput > 0 || Hero.transform.position.x > this.gameObject.transform.position.x)
                {
                    mySprite.flipX = false;
                    firePoint.transform.rotation = Quaternion.Euler(0, 0, -90);
                }
            }
            else if (leftFacingSprite==true)
            {
                if (horizInput < 0 || Hero.transform.position.x < this.gameObject.transform.position.x)
                {
                    mySprite.flipX = false;
                    firePoint.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                if (horizInput > 0 || Hero.transform.position.x > this.gameObject.transform.position.x)
                {
                    mySprite.flipX = true;
                    firePoint.transform.rotation = Quaternion.Euler(0, 0, -90);
                }
            }

        }


    }
    new public void Attack()
    {
        Animator.SetBool("Attacking", true);
        attacking = true;
        attackStart = Time.time;          
    }

    new public void AttackHit()
    {
        attackFired = true;
        currentProjectile = Instantiate(projectile, firePoint.transform.position, firePoint.transform.rotation) as GameObject;
        currentProjectile.GetComponent<Arrow>().FindStuff(this.gameObject, this.GetComponent<OverworldBanditArcher>());
        currentProjectile.transform.parent = this.transform;
        if(leftFacingSprite==false)
        {
            if (mySprite.flipX == true)
            {
                currentProjectile.GetComponent<Rigidbody2D>().velocity = new Vector2(-5, 0);
            }
            else if (mySprite.flipX == false)
            {
                currentProjectile.GetComponent<Rigidbody2D>().velocity = new Vector2(5, 0);
            }
        }
        if(leftFacingSprite==true)
        {
            if (mySprite.flipX == false)
            {
                currentProjectile.GetComponent<Rigidbody2D>().velocity = new Vector2(-5, 0);
            }
            else if (mySprite.flipX == true)
            {
                currentProjectile.GetComponent<Rigidbody2D>().velocity = new Vector2(5, 0);
            }
        }

    }

    new public void StopAttack()
    {
        attacking = false;
        Animator.SetBool("Attacking", false);
        canAttack = false;
        attackFired = false;
        attackCooldownStart = Time.time;

    }
}

