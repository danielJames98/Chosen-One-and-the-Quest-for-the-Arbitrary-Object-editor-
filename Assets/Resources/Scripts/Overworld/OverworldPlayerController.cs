using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldPlayerController : MonoBehaviour
{
    Animator Animator;

    public float currentWalkForce;
    public float airControlForce;
    public float defaultWalkForce;
    public float crouchingWalkForce;
    public float jumpForce;
    public float maxSpeed;
    public float horizInput;
    public Transform groundedCheck;
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
    public CapsuleCollider2D Collider;
    public AudioSource audioSource;
    public SpriteRenderer mySprite;
    public float crouchCorrectForce;
    public bool dashAdvantage;
    public float dashAdvantageDuration;

    public bool attacking;
    public float attackStart;
    public float attackDuration;
    public GameObject attackHitbox;
    public Animator attackHitBoxAnimator;
    public BoxCollider2D attackHitboxCollider;
    public bool enemyHit;
    public float attackHitTime;

    public bool blocking;
    public float blockStart;
    public float blockDuration;

    public PlayerState playerState;
    public BaseHero Hero0;
    public BaseHero Hero1;
    public BaseHero Hero2;
    public BaseHero Hero3;

    public int curXP;
    public int curLevel;

    public GameObject followCamera;
    public FollowCam cameraScript;

    

    void Awake()
    {
        //initialisation
        Animator = GetComponent<Animator>();
        Collider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        mySprite = GetComponent<SpriteRenderer>();

        

        playerState = GameObject.Find("PlayerState").GetComponent<PlayerState>();
        Hero0 = this.transform.Find("Hero0").GetComponent<BaseHero>();
        Hero1 = this.transform.Find("Hero1").GetComponent<BaseHero>();
        Hero2 = this.transform.Find("Hero2").GetComponent<BaseHero>();
        Hero3 = this.transform.Find("Hero3").GetComponent<BaseHero>();


        attackHitbox = GameObject.Find("AttackHitbox");
        attackHitBoxAnimator = attackHitbox.GetComponent<Animator>();
        attackHitboxCollider = attackHitbox.GetComponent<BoxCollider2D>();
        attackHitbox.GetComponent<AttackHitbox>().FindStuff(this.gameObject, this.gameObject.GetComponent<OverworldPlayerController>());
        attackHitbox.SetActive(false);
        followCamera= GameObject.Find("Main Camera");
        cameraScript = followCamera.GetComponent<FollowCam>();
        cameraScript.target = this.gameObject.GetComponent<Transform>();


        LevelCheck();

    }

    void Update()
    {
        horizInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        //checks to see if player is on ground
        grounded = Physics2D.Linecast(transform.position, groundedCheck.position, 1 << LayerMask.NameToLayer("Level"));

        if (horizInput < 0)
        {
            mySprite.flipX = true;
        }
        if (horizInput > 0)
        {
            mySprite.flipX = false;
        }

        if (Input.GetButtonDown("Jump") && grounded == true)
        {
            Jump();
        }

        if (Input.GetButtonDown("Jump") && grounded == false && hasAirJumped == false)
        {
            AirJump();
        }

        if (dashing == false && attacking == false && blocking == false)
        {
            Walk();
        }

        if (jumping == true && grounded == true && ((Time.time - jumpStartTime) > 0.1))
        {
            jumping = false;
            Animator.SetBool("Jumping", false);
            hasAirJumped = false;
        }

        if (Input.GetButtonDown("Dash") && canDash == true)
        {
            Dash();
        }

        if ((Time.time - dashStartTime) > dashDuration)
        {
            dashing = false;
        }

        if ((Time.time - dashStartTime) > dashAdvantageDuration)
        {
            dashAdvantage = false;
        }

        if ((Time.time - dashStartTime) > dashCooldown)
        {
            canDash = true;
        }

        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }

        if (Input.GetButtonDown("Block"))
        {
            Block();
        }

        if((attacking==true) && (Time.time-attackStart)>attackHitTime)
        {
            attackHitbox.SetActive(true);
            if (mySprite.flipX == false)
            {
                attackHitbox.transform.position = new Vector2(this.gameObject.transform.position.x + 0.3f, this.gameObject.transform.position.y);
            }
            else if (mySprite.flipX == true)
            {
                attackHitbox.transform.position = new Vector2(this.gameObject.transform.position.x - 0.3f, this.gameObject.transform.position.y);
            }
        }

        if ((attacking == true) && ((Time.time - attackStart) > attackDuration))
        {
            StopAttack();
        }

        if((blocking == true)&&((Time.time-blockStart)>blockDuration))
        {
            StopBlocking();
        }

        if (enemyHit == true)
        {
            EnterBattle();
            if(rb.velocity.y<0)
            {
                GameObject.Find("WorldState").GetComponent<WorldState>().advantageState = "Hero Drop Advantage";
            }
            else if (dashAdvantage==true)
            {
                GameObject.Find("WorldState").GetComponent<WorldState>().advantageState = "Hero Dash Advantage";
            }
            else
            {
                GameObject.Find("WorldState").GetComponent<WorldState>().advantageState = "Hero Advantage";
            }
            
            GameObject.Find("WorldState").GetComponent<WorldState>().heroReady = true;
        }
    }



    public void Walk()
    {

         rb.velocity = new Vector2(horizInput * currentWalkForce, rb.velocity.y);


        if (horizInput != 0)
        {
            Animator.SetBool("Walking", true);
        }
        if (horizInput == 0 && rb.velocity.x == 0)
        {
            Animator.SetBool("Walking", false);
        }
    }

    public void Jump()
    {
        jumping = true;
        Animator.SetBool("Jumping", true);
        jumpStartTime = Time.time;
        rb.AddForce(Vector2.up * jumpForce);
    }

    public void AirJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        hasAirJumped = true;
        jumping = true;
        Animator.SetBool("Jumping", true);
        jumpStartTime = Time.time;
        rb.AddForce(Vector2.up * jumpForce);
    }

    public void Dash()
    {
        dashing = true;
        dashAdvantage = true;
        if (grounded == false)
        {
            if (mySprite.flipX == false)
            {
                rb.AddForce(Vector2.right * dashForce);
            }

            if (mySprite.flipX == true)
            {
                rb.AddForce(Vector2.left * dashForce);
            }
        }

        if (grounded == true)
        {
            if (mySprite.flipX == false)
            {
                rb.AddForce(Vector2.right * dashForce * 2);
            }

            if (mySprite.flipX == true)
            {
                rb.AddForce(Vector2.left * dashForce * 2);
            }
        }



        canDash = false;
        dashStartTime = Time.time;
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "EnemyAttack" && blocking == true)
        {
            Debug.Log("Attack Blocked");
        }
        else if (collision.gameObject.tag=="EnemyAttack" && blocking == false)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().enemyInBattle = collision.gameObject.transform.parent.gameObject;
            EnterBattle();
        }
        else if (collision.gameObject.name=="CameraLocker" &&cameraScript.slowFollow==true)
        {
            cameraScript.slowFollow = false;
        }
        else if (collision.gameObject.name == "CameraUnlocker" && cameraScript.slowFollow == false)
        {
            cameraScript.slowFollow = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EnemyAttack" && blocking ==true)
        {
            Debug.Log("Attack Blocked");
        }
        else if (collision.gameObject.tag == "EnemyAttack" && blocking == false)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().enemyInBattle = collision.gameObject.transform.parent.gameObject;
            EnterBattle();
        }
        else if (collision.gameObject.name == "CameraLocker" && cameraScript.slowFollow == true)
        {
            cameraScript.slowFollow = false;
        }
        else if (collision.gameObject.name == "CameraUnlocker" && cameraScript.slowFollow == false)
        {
            cameraScript.slowFollow = true;
        }
    }



    void Attack()
    {
        attacking = true;       
        Animator.SetBool("Attacking", true);
        attackStart = Time.time;
    }

    void Block()
    {
        blocking = true;
        Animator.SetBool("Blocking", true);
        blockStart = Time.time;
    }

    void StopAttack()
    {
        Animator.SetBool("Attacking", false);
        
        attacking = false;
        attackHitbox.SetActive(false);

    }

    void StopBlocking()
    {
        Animator.SetBool("Blocking", false);
        blocking = false;
    }

    void EnterBattle()
    {
        LevelCheck();

        playerState.transform.Find("Hero0").GetComponent<BaseHero>().characterName = Hero0.characterName;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().spritePath = Hero0.spritePath;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().animatorPath = Hero0.animatorPath;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().forwardAbilitySource = Hero0.forwardAbilitySource;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().overheadAbilitySource = Hero0.overheadAbilitySource;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().maxHP = Hero0.maxHP;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().curHP = Hero0.curHP;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().curAP = Hero0.curAP;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().baseSpeed = Hero0.baseSpeed;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().curSpeed = Hero0.curSpeed;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().basePower = Hero0.basePower;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().curPower = Hero0.curPower;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().level = Hero0.level;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().lvl1HP = Hero0.lvl1HP;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().lvl1Speed = Hero0.lvl1Speed;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().lvl1Power = Hero0.lvl1Power;
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().attacks[0] = Hero0.attacks[0];
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().attacks[1] = Hero0.attacks[1];
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().attacks[2] = Hero0.attacks[2];
        playerState.transform.Find("Hero0").GetComponent<BaseHero>().attacks[3] = Hero0.attacks[3];

        playerState.transform.Find("Hero1").GetComponent<BaseHero>().characterName = Hero1.characterName;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().spritePath = Hero1.spritePath;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().animatorPath = Hero1.animatorPath;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().forwardAbilitySource = Hero1.forwardAbilitySource;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().overheadAbilitySource = Hero1.overheadAbilitySource;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().maxHP = Hero1.maxHP;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().curHP = Hero1.curHP;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().curAP = Hero1.curAP;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().baseSpeed = Hero1.baseSpeed;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().curSpeed = Hero1.curSpeed;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().basePower = Hero1.basePower;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().curPower = Hero1.curPower;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().level = Hero1.level;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().lvl1HP = Hero1.lvl1HP;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().lvl1Speed = Hero1.lvl1Speed;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().lvl1Power = Hero1.lvl1Power;
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().attacks[0] = Hero1.attacks[0];
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().attacks[1] = Hero1.attacks[1];
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().attacks[2] = Hero1.attacks[2];
        playerState.transform.Find("Hero1").GetComponent<BaseHero>().attacks[3] = Hero1.attacks[3];

        playerState.transform.Find("Hero2").GetComponent<BaseHero>().characterName = Hero2.characterName;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().spritePath = Hero2.spritePath;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().animatorPath = Hero2.animatorPath;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().forwardAbilitySource = Hero2.forwardAbilitySource;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().overheadAbilitySource = Hero2.overheadAbilitySource;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().maxHP = Hero2.maxHP;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().curHP = Hero2.curHP;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().curAP = Hero2.curAP;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().baseSpeed = Hero2.baseSpeed;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().curSpeed = Hero2.curSpeed;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().basePower = Hero2.basePower;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().curPower = Hero2.curPower;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().level = Hero2.level;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().lvl1HP = Hero2.lvl1HP;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().lvl1Speed = Hero2.lvl1Speed;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().lvl1Power = Hero2.lvl1Power;
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().attacks[0] = Hero2.attacks[0];
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().attacks[1] = Hero2.attacks[1];
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().attacks[2] = Hero2.attacks[2];
        playerState.transform.Find("Hero2").GetComponent<BaseHero>().attacks[3] = Hero2.attacks[3];

        playerState.transform.Find("Hero3").GetComponent<BaseHero>().characterName = Hero3.characterName;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().spritePath = Hero3.spritePath;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().animatorPath = Hero3.animatorPath;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().forwardAbilitySource = Hero3.forwardAbilitySource;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().overheadAbilitySource = Hero3.overheadAbilitySource;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().maxHP = Hero3.maxHP;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().curHP = Hero3.curHP;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().curAP = Hero3.curAP;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().baseSpeed = Hero3.baseSpeed;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().curSpeed = Hero3.curSpeed;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().basePower = Hero3.basePower;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().curPower = Hero3.curPower;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().level = Hero3.level;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().lvl1HP = Hero3.lvl1HP;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().lvl1Speed = Hero3.lvl1Speed;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().lvl1Power = Hero3.lvl1Power;
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().attacks[0] = Hero3.attacks[0];
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().attacks[1] = Hero3.attacks[1];
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().attacks[2] = Hero3.attacks[2];
        playerState.transform.Find("Hero3").GetComponent<BaseHero>().attacks[3] = Hero3.attacks[3];

        playerState.Level = curLevel;

        GameObject.Find("WorldState").GetComponent<WorldState>().heroReady = true;
    }

    public void LevelCheck()
    {
        if (curLevel == 0)
        {
            curLevel = 1;
        }

        if (curXP== curLevel*100)
        {
            curLevel++;
            curXP = 0;
        }

        Hero0.level = curLevel;
        Hero1.level = curLevel;
        Hero2.level = curLevel;
        Hero3.level = curLevel;
    }
}



