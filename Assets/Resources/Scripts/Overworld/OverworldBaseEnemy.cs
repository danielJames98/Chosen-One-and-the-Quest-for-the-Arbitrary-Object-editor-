using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldBaseEnemy : MonoBehaviour
{
    public Animator Animator;

    public GameObject prefab;
    public Transform groundedCheck;
    public float currentWalkForce;
    public float defaultWalkForce;
    public float crouchingWalkForce;
    public float jumpForce;
    public float maxSpeed;
    public float horizInput;
    public bool grounded;
    public bool jumping;
    public float jumpStartTime;
    public bool hasAirJumped;
    public float dashForce;
    public bool canDash;
    public float dashStartTime;
    public float dashCooldown;
    public bool dashing;
    public float dashDuration;
    public bool crouching;
    public Rigidbody2D rb;
    public CapsuleCollider2D capsuleCollider;
    public AudioSource audioSource;
    public SpriteRenderer mySprite;
    public float crouchCorrectForce;

    public bool attacking;
    public float attackStart;
    public float attackDuration;
    public float attackHitTime;
    public GameObject attackHitbox;
    public Animator attackHitBoxAnimator;
    public BoxCollider2D attackHitboxCollider;
    public bool heroHit;

    public EnemyState enemyState;
    public BaseEnemy Enemy0;
    public BaseEnemy Enemy1;
    public BaseEnemy Enemy2;
    public BaseEnemy Enemy3;

    public bool inCombat;
    public GameObject Hero;
    public bool heroSpotted;
    public bool heroInRange;

    public GameObject Sensor;
    public BoxCollider2D sensorCollider;

    public GameObject rangeSensor;
    public BoxCollider2D rangeSensorCollider;

    public GameObject chaseSensor;

    public float attackCooldown;
    public float attackCooldownStart;
    public bool canAttack;
    public bool attackFired;

    public string prefabName;

    public int curXP;
    public int curLevel;

    public bool leftFacingSprite;

    public float attackHitboxXOffset;

    public void Awake()
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

        if(gameObject.transform.Find("enemyAttackHitbox") !=null)
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
}

    public void FixedUpdate()
    {
        //checks to see if player is on ground
        grounded = Physics2D.Linecast(transform.position, groundedCheck.position, 1 << LayerMask.NameToLayer("Level"));

        DirectionCheck();


        if (inCombat == true && heroInRange == false)
        {
            Chase();
        }

        if (attacking == true && (Time.time - attackStart) > attackHitTime && attackFired==false)
        {
            AttackHit();
        }
        
        if ((attacking == true) && ((Time.time - attackStart) > attackDuration))
        {
            StopAttack();
        }

        if (heroInRange==true && attacking==false && canAttack==true)
        {
            Attack();
        }




        if (heroHit == true)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().advantageState = "Enemy Advantage";
            SendParty();     
        }

        if (attacking==true)
        {
            if (rb.velocity.x>0)
            {
                rb.velocity = new Vector2(rb.velocity.x-0.1f, rb.velocity.y);
            }

            if (rb.velocity.x < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + 0.1f, rb.velocity.y);
            }
        }

        if((Time.time-attackCooldownStart)>attackCooldown)
        {
            canAttack = true;
        }

        if (inCombat == false)
        {
            Idle();
            
        }

        if (jumping == true && grounded == true && ((Time.time - jumpStartTime) > 1))
        {
            jumping = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "HeroAttack")
        {
            StopMoving();
            StopAttack();
            GameObject.Find("WorldState").GetComponent<WorldState>().enemyInBattle = this.gameObject;
            SendParty();
        }

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "HeroAttack")
        {
            StopMoving();
            StopAttack();
            GameObject.Find("WorldState").GetComponent<WorldState>().enemyInBattle = this.gameObject;
            SendParty();
        }

    }

    public void HeroSpotted()
    {
        inCombat = true;
        heroSpotted = true;
    }
    public void StopMoving()
    {
        Animator.SetInteger("AnimState", 0);
        rb.velocity = new Vector2(0,0);
    }

    public void Chase()
    {
        Animator.SetInteger("AnimState", 2);


        if (Hero.transform.position.x > this.gameObject.transform.position.x && horizInput < 1)
        {
            horizInput += 0.05f;
        }
        else if (Hero.transform.position.x < this.gameObject.transform.position.x && horizInput>-1)
        {
            horizInput -=0.05f;
        }

        rb.velocity = new Vector2(horizInput * currentWalkForce, rb.velocity.y);
    }

    
    public void Attack()
    {
        Animator.SetTrigger("Attack");
        attacking = true;
        
        attackStart = Time.time;
    }
    

    public void StopAttack()
    {
        attacking = false;
        if(attackHitbox!=null)
        {
            attackHitbox.SetActive(false);
        }
        attackFired = false;
        canAttack = false;
        attackCooldownStart = Time.time;

    }

    public void StopChase()
    {
        heroSpotted = false;
        inCombat = false;
        Hero = null;
        Animator.SetInteger("AnimState", 0);
    }

    public void Jump(float force)
    {
        if(Hero!=null)
        {
            if (jumping == false)
            {
                jumping = true;
                jumpStartTime = Time.time;
                rb.AddForce(Vector2.up * force);
            }
        }


    }

    public void Idle()
    {
        Animator.SetInteger("AnimState", 0);
    }

    public void DirectionCheck()
    {
        if (leftFacingSprite==true)
        {
            if (horizInput < 0)
            {
                mySprite.flipX = false;

            }
            if (horizInput > 0)
            {
                mySprite.flipX = true;
            }
        }
        else
        {
            if (horizInput < 0)
            {
                mySprite.flipX = true;

            }
            if (horizInput > 0)
            {
                mySprite.flipX = false;
            }
        }

    }

    public void AttackHit()
    {
        attackFired = true;
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);

            if (leftFacingSprite==true)
            {
                if (mySprite.flipX == false)
                {
                    attackHitbox.transform.position = new Vector2((this.gameObject.transform.position.x - attackHitboxXOffset), this.gameObject.transform.position.y);
                }

                if (mySprite.flipX == true)
                {
                    attackHitbox.transform.position = new Vector2((this.gameObject.transform.position.x + attackHitboxXOffset), this.gameObject.transform.position.y);
                }
            }
            else
            {
                if (mySprite.flipX == true)
                {
                    attackHitbox.transform.position = new Vector2((this.gameObject.transform.position.x - attackHitboxXOffset), this.gameObject.transform.position.y);
                }

                if (mySprite.flipX == false)
                {
                    attackHitbox.transform.position = new Vector2((this.gameObject.transform.position.x + attackHitboxXOffset), this.gameObject.transform.position.y);
                }
            }


        }


    }

    public void SendParty()
    {
        LevelCheck();

        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().characterName = Enemy0.characterName;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().spritePath = Enemy0.spritePath;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().animatorPath = Enemy0.animatorPath;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().leftFacingSprite = Enemy0.leftFacingSprite;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().spriteScale = Enemy0.spriteScale;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().yOffset = Enemy0.yOffset;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().forwardAbilitySource = Enemy0.forwardAbilitySource;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().overheadAbilitySource = Enemy0.overheadAbilitySource;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().maxHP = Enemy0.maxHP;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().curHP = Enemy0.curHP;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().curAP = Enemy0.curAP;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().baseSpeed = Enemy0.baseSpeed;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().curSpeed = Enemy0.curSpeed;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().basePower = Enemy0.basePower;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().curPower = Enemy0.curPower;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().level = Enemy0.level;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().lvl1HP = Enemy0.lvl1HP;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().lvl1Speed = Enemy0.lvl1Speed;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().lvl1Power = Enemy0.lvl1Power;
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().attacks[0] = Enemy0.attacks[0];
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().attacks[1] = Enemy0.attacks[1];
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().attacks[2] = Enemy0.attacks[2];
        enemyState.transform.Find("Enemy0").GetComponent<BaseEnemy>().attacks[3] = Enemy0.attacks[3];

        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().characterName = Enemy1.characterName;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().spritePath = Enemy1.spritePath;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().animatorPath = Enemy1.animatorPath;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().leftFacingSprite = Enemy1.leftFacingSprite;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().spriteScale = Enemy1.spriteScale;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().yOffset = Enemy1.yOffset;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().forwardAbilitySource = Enemy1.forwardAbilitySource;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().overheadAbilitySource = Enemy1.overheadAbilitySource;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().maxHP = Enemy1.maxHP;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().curHP = Enemy1.curHP;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().curAP = Enemy1.curAP;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().baseSpeed = Enemy1.baseSpeed;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().curSpeed = Enemy1.curSpeed;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().basePower = Enemy1.basePower;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().curPower = Enemy1.curPower;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().level = Enemy1.level;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().lvl1HP = Enemy1.lvl1HP;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().lvl1Speed = Enemy1.lvl1Speed;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().lvl1Power = Enemy1.lvl1Power;
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().attacks[0] = Enemy1.attacks[0];
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().attacks[1] = Enemy1.attacks[1];
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().attacks[2] = Enemy1.attacks[2];
        enemyState.transform.Find("Enemy1").GetComponent<BaseEnemy>().attacks[3] = Enemy1.attacks[3];

        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().characterName = Enemy2.characterName;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().spritePath = Enemy2.spritePath;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().animatorPath = Enemy2.animatorPath;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().leftFacingSprite = Enemy2.leftFacingSprite;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().spriteScale = Enemy2.spriteScale;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().yOffset = Enemy2.yOffset;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().forwardAbilitySource = Enemy2.forwardAbilitySource;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().overheadAbilitySource = Enemy2.overheadAbilitySource;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().maxHP = Enemy2.maxHP;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().curHP = Enemy2.curHP;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().curAP = Enemy2.curAP;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().baseSpeed = Enemy2.baseSpeed;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().curSpeed = Enemy2.curSpeed;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().basePower = Enemy2.basePower;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().curPower = Enemy2.curPower;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().level = Enemy2.level;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().lvl1HP = Enemy2.lvl1HP;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().lvl1Speed = Enemy2.lvl1Speed;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().lvl1Power = Enemy2.lvl1Power;
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().attacks[0] = Enemy2.attacks[0];
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().attacks[1] = Enemy2.attacks[1];
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().attacks[2] = Enemy2.attacks[2];
        enemyState.transform.Find("Enemy2").GetComponent<BaseEnemy>().attacks[3] = Enemy2.attacks[3];

        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().characterName = Enemy3.characterName;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().spritePath = Enemy3.spritePath;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().animatorPath = Enemy3.animatorPath;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().leftFacingSprite = Enemy3.leftFacingSprite;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().spriteScale = Enemy3.spriteScale;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().yOffset = Enemy3.yOffset;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().forwardAbilitySource = Enemy3.forwardAbilitySource;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().overheadAbilitySource = Enemy3.overheadAbilitySource;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().maxHP = Enemy3.maxHP;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().curHP = Enemy3.curHP;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().curAP = Enemy3.curAP;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().baseSpeed = Enemy3.baseSpeed;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().curSpeed = Enemy3.curSpeed;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().basePower = Enemy3.basePower;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().curPower = Enemy3.curPower;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().level = Enemy3.level;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().lvl1HP = Enemy3.lvl1HP;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().lvl1Speed = Enemy3.lvl1Speed;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().lvl1Power = Enemy3.lvl1Power;
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().attacks[0] = Enemy3.attacks[0];
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().attacks[1] = Enemy3.attacks[1];
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().attacks[2] = Enemy3.attacks[2];
        enemyState.transform.Find("Enemy3").GetComponent<BaseEnemy>().attacks[3] = Enemy3.attacks[3];
        GameObject.Find("WorldState").GetComponent<WorldState>().enemyReady = true;

        enemyState.level = curLevel;
    }

    public void LevelCheck()
    {
        if (curLevel == 0)
        {
            curLevel = 1;
        }

        if (curXP == curLevel * 100)
        {
            curLevel++;
            curXP = 0;
        }

        Enemy0.level = curLevel;
        Enemy1.level = curLevel;
        Enemy2.level = curLevel;
        Enemy3.level = curLevel;

        
    }

    public void DestroySelf()
    {
        DestroyImmediate(this.gameObject, true);
    }
}
