using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{
    public BattleStateMachine BSM;
    public BaseHero hero;
    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;
    //for ap
    public float cur_AP;
    public GameObject Selector;

    //ienumerator
    public GameObject Target;
    private bool actionStarted = false;
    private Vector3 startPosition;
    public float animSpeed = 10;

    //death
    private bool alive =  true;

    //status effects
    float calc_damage;
    public int weakened;
    public int block;
    public int taunt;
    public int strengthened;
    public int stealth;
    public int vulnerable;
    public int shielded;
    public int hastened;
    public int slowed;
    public List<int> dotDurations = new List<int>();
    public List<int> dotDamages = new List<int>();

    public Animator animator;
    public Animator abilityVisualsAnimator;

    public string advantageState;

    public Text combatText;

    void Start()
    {
        hero = this.GetComponent<BaseHero>();
        startPosition = transform.position;
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
        cur_AP = 0;
        ApplyLevels();
        AdvantageEffects();
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(hero.spritePath);
        this.gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(hero.animatorPath);
        animator = this.gameObject.GetComponent<Animator>();
        abilityVisualsAnimator = this.gameObject.transform.Find("AbilityVisuals").GetComponent<Animator>();
        combatText = GameObject.Find("CombatText").GetComponent<Text>();
    }

    void Update()
    {

        switch (currentState)
        {
            case (TurnState.PROCESSING):
                if ((BSM.PerformList.Count == 0) && (BSM.HeroesToManage.Count == 0) && (BSM.BattleOver == false))
                {
                    UpdateProgressBar();
                }
                break;
            case (TurnState.ADDTOLIST):
                BSM.HeroesToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;
            case (TurnState.WAITING):
                //idle
                break;
            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
            case (TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    //change tag
                    this.gameObject.tag = "DeadHero";
                    //not attackable
                    //not managable
                    BSM.HeroesToManage.Remove(this.gameObject);
                    //deactiavte slector
                    Selector.SetActive(false);
                    //reset ui
                    BSM.ActionButtons.Clear();
                    BSM.TargetButtons.Clear();
                    BSM.ActionPanel.SetActive(false);
                    BSM.TargetSelectPanel.SetActive(false);
                    //remove from perform list
                    for(int i = 0; i<BSM.PerformList.Count; i++)
                    {
                        if(BSM.PerformList[i].AttackersGameObject== this.gameObject)
                        {
                            BSM.PerformList.Remove(BSM.PerformList[i]);
                        }
                    }
                    //death anim
                    
                    //reset hero input
                    BSM.HeroInput = BattleStateMachine.HeroGUI.ACTIVATE;
                    alive = false;
                }
                break;

        }
    }

    void UpdateProgressBar()
    {
        cur_AP = cur_AP + (Time.deltaTime*hero.curSpeed);
        hero.curAP = cur_AP;
        if (cur_AP>=100)
        {
            cur_AP = 100;
            hero.curAP = 100;
            currentState = TurnState.ADDTOLIST;
        }
    }

    public IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        Vector3 enemyPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        if (BSM.HeroChoice.AttackersTarget!= BSM.HeroChoice.AttackersGameObject && BSM.HeroChoice.chosenAttack.ranged ==false)
        {
            enemyPosition = new Vector3(Target.transform.position.x - 3f, this.transform.position.y, this.transform.position.z);
        }
        
        while (MoveTowardsEnemy(enemyPosition))
        {
            yield return null;
        }
        
       
        animator.SetBool("Running", false);
        

        ActionAnimation();
       
        yield return new WaitForSeconds(1f);

        ApplyActionEffects();
        combatText.text = hero.characterName + " used " + BSM.PerformList[0].chosenAttack.abilityName + "\n" + BSM.PerformList[0].chosenAttack.abilityDescription;

        animator.SetBool("Attacking", false);

        //animate back to start position
        Vector3 firstPosition = startPosition;

        if(BSM.HeroChoice.AttackersTarget != BSM.HeroChoice.AttackersGameObject && BSM.HeroChoice.chosenAttack.ranged == false)
        {
            flip();
        }
        
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }

        animator.SetBool("Running", false);

        if (BSM.HeroChoice.AttackersTarget != BSM.HeroChoice.AttackersGameObject && BSM.HeroChoice.chosenAttack.ranged == false)
        {
            flip();
        }

        //remove performer from list in BSM
        BSM.PerformList.RemoveAt(0);

        //reset BSM to wait
        BSM.battleStates = BattleStateMachine.PerformAction.WAIT;

        //end coroutine
        actionStarted = false;

        //reset this enemy state
        cur_AP -= 100f;
        hero.curAP -= 100f;
        currentState = TurnState.PROCESSING;

        hero.curPower = hero.basePower;
        hero.curSpeed = hero.baseSpeed;

        ApplyDotDamage();
    }

    public bool MoveTowardsEnemy(Vector3 target)
    {
        if (BSM.HeroChoice.AttackersTarget != BSM.HeroChoice.AttackersGameObject && BSM.HeroChoice.chosenAttack.ranged == false)
        {
            animator.SetBool("Running", true);
        }
        
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }
    public bool MoveTowardsStart(Vector3 target)
    {
        
        if (BSM.HeroChoice.AttackersTarget != BSM.HeroChoice.AttackersGameObject && BSM.HeroChoice.chosenAttack.ranged == false)
        {
            animator.SetBool("Running", true);
        }
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        if (BSM.PerformList[0].chosenAttack.abilityDamage!=0)
        {
            calc_damage = hero.curPower + BSM.PerformList[0].chosenAttack.abilityDamage + Random.Range(-5, 5);
        }
        

        if (weakened > 0)
        {
            calc_damage = (calc_damage/2);
            weakened--;
        }

        if (strengthened>0)
        {
            calc_damage = (calc_damage *2);
            strengthened--;
        }

        if (stealth>0)
        {
            calc_damage = (calc_damage * 2);
            stealth--;
        }

        if (BSM.PerformList[0].chosenAttack.dotDuration > 0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().dotDurations.Add(BSM.PerformList[0].chosenAttack.dotDuration);
            BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().dotDamages.Add(BSM.PerformList[0].chosenAttack.dotDamage);
        }

        if(BSM.PerformList[0].chosenAttack.weakened>0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().weakened = BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().weakened + BSM.PerformList[0].chosenAttack.weakened;
        }

        if (BSM.PerformList[0].chosenAttack.vulnerable > 0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().vulnerable = BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().weakened + BSM.PerformList[0].chosenAttack.vulnerable;
        }

        Target.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
    }

    void Cleave()
    {
        calc_damage = hero.curPower + BSM.PerformList[0].chosenAttack.abilityDamage + Random.Range(-5, 5);

        if (weakened > 0)
        {
            calc_damage = (calc_damage / 2);
            weakened--;
        }

        if (strengthened > 0)
        {
            calc_damage = (calc_damage * 2);
            strengthened--;
        }

        if (stealth > 0)
        {
            calc_damage = (calc_damage * 2);
            stealth--;
        }

        foreach (GameObject enemy in BSM.EnemiesInBattle)
        {
            if (enemy.tag != "DeadEnemy")
            {
                enemy.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
            }
        }
    }

    void Heal()
    {
        float calc_healing = hero.curPower + BSM.PerformList[0].chosenAttack.abilityDamage;

        if (weakened>0)
        {
            calc_healing = (calc_healing / 2);
            weakened--;
        }

        if(strengthened>0)
        {
            calc_healing = (calc_healing * 2);
            strengthened--;
        }
        Target.GetComponent<HeroStateMachine>().TakeHealing(calc_healing);

    }

    void Buff()
    {
        Target.GetComponent<HeroStateMachine>().block += BSM.PerformList[0].chosenAttack.block;
        Target.GetComponent<HeroStateMachine>().taunt += BSM.PerformList[0].chosenAttack.taunt;
        Target.GetComponent<HeroStateMachine>().strengthened += BSM.PerformList[0].chosenAttack.strengthened;
        Target.GetComponent<HeroStateMachine>().stealth = BSM.PerformList[0].chosenAttack.stealth;
        Target.GetComponent<HeroStateMachine>().hero.curSpeed += (BSM.PerformList[0].chosenAttack.hastened);
    }

    void Aura()
    {
        foreach (GameObject hero in BSM.HeroesInBattle)
        {
            if (hero.tag != "DeadHero")
            {
                hero.GetComponent<HeroStateMachine>().strengthened += BSM.PerformList[0].chosenAttack.strengthened;
            }
        }
    }

    public void TakeDamage(float getDamageAmount)
    {
        animator.SetTrigger("Taking Hit");
        if (BSM.PerformList[0].chosenAttack.stun>0)
        {
            cur_AP = (cur_AP - (BSM.PerformList[0].chosenAttack.stun * 50));
            hero.curAP = (hero.curAP - (BSM.PerformList[0].chosenAttack.stun * 50));
        }

        if (BSM.PerformList[0].chosenAttack.slowed>0)
        {
            hero.curSpeed -= BSM.PerformList[0].chosenAttack.slowed;
        }

        if (block>0)
        {
            getDamageAmount = (getDamageAmount/2);
            block--;
        }

        if (shielded>0)
        {
            getDamageAmount = (getDamageAmount / 2);
            shielded--;
        }

        if(vulnerable>0)
        {
            getDamageAmount = (getDamageAmount * 1.2f);
            vulnerable--;
        }

            hero.curHP -= getDamageAmount;

        if (hero.curHP<=0)
        {
            hero.curHP = 0;
            animator.SetBool("Dead", true);
            BSM.HeroesAlive--;
            currentState = TurnState.DEAD;
        }
    }

    public void ApplyDotDamage()
    {
        if (dotDamages.Count>0)
        {
            int i = 0;
            foreach (int dot in dotDamages)
            {
                hero.curHP -= dot;
                dotDurations[i]--;
                if (dotDurations[i] < 1)
                {
                    dotDurations.RemoveAt(i);
                    dotDamages.RemoveAt(i);
                }
                i++;
            }
        }

    }

    public void TakeHealing(float getHealingAmount)
    {
        hero.curHP += getHealingAmount;
        if (hero.curHP >= hero.maxHP)
        {
            hero.curHP = hero.maxHP;
        }
    }

    public void AdvantageEffects()
    {
        if (advantageState == "Hero Advantage")
        {

        }

        if (advantageState == "Hero Drop Advantage")
        {

        }

        if (advantageState == "Hero Dash Advantage")
        {
            cur_AP = 50;
            hero.curAP = 50;
        }

        if (advantageState == "Enemy Advantage")
        {
            hero.curHP -= 10;
        }


    }

    public void flip()
    {
        if (this.gameObject.GetComponent<SpriteRenderer>().flipX == false)
        {
            this.gameObject.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (this.gameObject.GetComponent<SpriteRenderer>().flipX == true)
        {
            this.gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    public void ApplyActionEffects()
    {
        // do damage
        if (BSM.HeroChoice.chosenAttack.abilityType == "Attack")
        {
            DoDamage();
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Buff")
        {            
            Buff();
            if(BSM.PerformList[0].chosenAttack.abilityDamage>0)
            {
                Heal();
            }
            
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Stance")
        {
            Buff();
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Aura")
        {
            Aura();
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Cleave")
        {

            Cleave();
        }
    }

    /*
    public void ActionAnimation()
    {
        if (BSM.HeroChoice.chosenAttack.abilityType == "Attack")
        {
            animator.SetBool("Attacking", true);
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Buff")
        {
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Stance")
        {
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Aura")
        {
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }
        else if (BSM.HeroChoice.chosenAttack.abilityType == "Cleave")
        {
            animator.SetBool("Attacking", true);
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }
    }
    */

    public void ActionAnimation()
    {
        if (BSM.HeroChoice.chosenAttack.abilityType == "Attack")
        {
            animator.SetBool("Attacking", true);
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }

        if (BSM.HeroChoice.chosenAttack.abilityType == "Buff")
        {
            
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }

        if (BSM.HeroChoice.chosenAttack.abilityType == "Stance")
        {
            
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }

        if (BSM.HeroChoice.chosenAttack.abilityType == "Aura")
        {
            
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }

        if (BSM.HeroChoice.chosenAttack.abilityType == "Cleave")
        {
            animator.SetBool("Attacking", true);
            abilityVisualsAnimator.SetTrigger(BSM.HeroChoice.chosenAttack.abilityName);
        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Barrier")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "BattleCry")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Block")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Bludgeon")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Electrocute")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Empower")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Fireball")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Freeze")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Heal")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "MagicMissile")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "MarkTarget")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Slash")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Smash")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Smite")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Sneak")
        {
            //this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 225);
        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "Taunt")
        {

        }

        if (BSM.HeroChoice.chosenAttack.abilityName == "ArrowShot")
        {

        }
    }

    public void ApplyLevels()
    {
        hero.curHP = hero.lvl1HP + ((hero.lvl1HP * 0.1f)*(hero.level-1));
        hero.maxHP = hero.lvl1HP + ((hero.lvl1HP * 0.1f) * (hero.level - 1));
        hero.baseSpeed = hero.lvl1Speed + ((hero.lvl1Speed * 0.1f) * (hero.level - 1));
        hero.curSpeed = hero.lvl1Speed + ((hero.lvl1Speed * 0.1f) * (hero.level - 1));
        hero.basePower = hero.lvl1Power + ((hero.lvl1Power * 0.1f) * (hero.level - 1));
        hero.curPower = hero.lvl1Power + ((hero.lvl1Power * 0.1f) * (hero.level - 1));
    }
}
