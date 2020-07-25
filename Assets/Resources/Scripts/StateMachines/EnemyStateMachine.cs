using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStateMachine : MonoBehaviour
{
    public BattleStateMachine BSM;
    public BaseEnemy enemy;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;
    public float cur_AP;
    public Text APText;

    public Vector3 startPosition;
    public GameObject Selector;

    public bool actionStarted = false;

    public GameObject Target;
    public float animSpeed = 10;

    //death
    public bool alive = true;

    //characterpanel
    private CharacterBarStats stats;
    public GameObject CharacterBar;
    private Transform EnemyPanelSpacer;

    //status effects
    public float calc_damage;
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

    public List<GameObject> viableTargets;

    public string advantageState;

    public Animator animator;
    public Animator abilityVisualsAnimator;

    public Text combatText;

    void Start()
    {
        enemy = this.GetComponent<BaseEnemy>();
        currentState = TurnState.PROCESSING;
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        
        ApplyLevels();
        AdvantageEffects();
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(enemy.spritePath);
        this.gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(enemy.animatorPath);
        if (enemy.leftFacingSprite==false)
        {
            this.GetComponent<SpriteRenderer>().flipX = true;
        }
        this.gameObject.transform.localScale = new Vector3(enemy.spriteScale, enemy.spriteScale, 1);
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, enemy.yOffset, this.gameObject.transform.position.z);
        startPosition = transform.position;
        animator = this.gameObject.GetComponent<Animator>();
        abilityVisualsAnimator = this.gameObject.transform.Find("AbilityVisuals").GetComponent<Animator>();
        combatText = GameObject.Find("CombatText").GetComponent<Text>();
    }

    void Update()
    {

        switch (currentState)
        {
            case (TurnState.PROCESSING):
                if((BSM.PerformList.Count ==0)&&(BSM.HeroesToManage.Count==0)&&(BSM.BattleOver==false))
                {
                    UpdateProgressBar();
                }
                break;
            case (TurnState.CHOOSEACTION):
                ChooseAction();
                currentState = TurnState.WAITING;
                break;
            case (TurnState.WAITING):
                //idle
                if((BSM.PerformList.Count==0))
                {
                    currentState = TurnState.CHOOSEACTION;
                }
                break;
            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
            case (TurnState.DEAD):
                //change tag
                this.gameObject.tag = "DeadEnemy";
                //not attackable
                //BSM.EnemiesInBattle.Remove(this.gameObject);
                //deactiavte slector
                Selector.SetActive(false);
                //remove from perform list
                for (int i = 0; i < BSM.PerformList.Count; i++)
                {
                    if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                    {
                        BSM.PerformList.Remove(BSM.PerformList[i]);
                    }
                }
                //death anim
                this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 225);
                alive = false;

                break;

        }
    }

    void UpdateProgressBar()
    {
        cur_AP = cur_AP + (Time.deltaTime*enemy.curSpeed);
        enemy.curAP = cur_AP;
        if (cur_AP >= 100)
        {
            cur_AP = 100;
            enemy.curAP = 100;
            if(BSM.PerformList.Count==0)
            {
                currentState = TurnState.WAITING;
            }
        }
    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.characterName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;


       

        int num = Random.Range(0, enemy.attacks.Count);
        myAttack.chosenAttack = enemy.attacks[num];

        if (myAttack.chosenAttack.abilityType == "Stance")
        {
            myAttack.AttackersTarget = myAttack.AttackersGameObject;
        }
        else if (myAttack.chosenAttack.abilityType == "Attack")
        {
            foreach (GameObject hero in BSM.HeroesInBattle)
            {
                if (hero.GetComponent<HeroStateMachine>().taunt>0 && hero.tag!= "DeadHero")
                {
                    viableTargets.Add(hero);
                }
            }

            if (viableTargets.Count>0)
            {
                myAttack.AttackersTarget = viableTargets[Random.Range(0, viableTargets.Count)];
                myAttack.AttackersTarget.GetComponent<HeroStateMachine>().taunt--;
            }
            else
            {
                foreach (GameObject heroNoTaunt in BSM.HeroesInBattle)
                {
                    if (heroNoTaunt.tag != "DeadHero" && (heroNoTaunt.GetComponent<HeroStateMachine>().stealth==0))
                    {
                        viableTargets.Add(heroNoTaunt);
                        
                    }
                }
                GameObject selectedTarget = viableTargets[Random.Range(0, viableTargets.Count)].gameObject;
                Debug.Log(selectedTarget);
                myAttack.AttackersTarget = selectedTarget;
            }
            
        }
        else if (myAttack.chosenAttack.abilityType == "Buff")
        {
            myAttack.AttackersTarget = BSM.EnemiesInBattle[Random.Range(0, BSM.EnemiesInBattle.Count)];
            
        }
        else if (myAttack.chosenAttack.abilityType == "Aura")
        {
            myAttack.AttackersTarget = myAttack.AttackersGameObject;

        }

        BSM.CollectActions(myAttack);
        viableTargets.Clear();
    }

    public IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // animate the enemy near the hero to attack
        Vector3 heroPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        if (BSM.PerformList[0].AttackersTarget!= BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged==false)
        {
            heroPosition = new Vector3(Target.transform.position.x + 3f, this.transform.position.y, this.transform.position.z);
        }

        while(MoveTowardsEnemy(heroPosition))
        {
            yield return null;
        }
        //wait
        if (BSM.PerformList[0].AttackersTarget != BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged == false)
        {
            animator.SetBool("Running", false);
        }

        ActionAnimation();

        yield return new WaitForSeconds(1f);

        ApplyActionEffects();
        combatText.text = combatText.text = enemy.characterName + " used " + BSM.PerformList[0].chosenAttack.abilityName + "\n" + BSM.PerformList[0].chosenAttack.abilityDescription;

        animator.SetBool("Attacking", false);
        

        //animate back to start position
        Vector3 firstPosition = startPosition;

        if (BSM.PerformList[0].AttackersTarget != BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged == false)
        {
            flip();
        }

        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }

        
            animator.SetBool("Running", false);
        

        if (BSM.PerformList[0].AttackersTarget != BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged == false)
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
        enemy.curAP -= 100f;
        currentState = TurnState.PROCESSING;

        enemy.curPower = enemy.basePower;
        enemy.curSpeed = enemy.baseSpeed;

    }

    public bool MoveTowardsEnemy(Vector3 target)
    {
        if (BSM.PerformList[0].AttackersTarget != BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged == false)
        {
            animator.SetBool("Running", true);
        }
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }
    public bool MoveTowardsStart(Vector3 target)
    {
        if (BSM.PerformList[0].AttackersTarget != BSM.PerformList[0].AttackersGameObject && BSM.PerformList[0].chosenAttack.ranged == false)
        {
            animator.SetBool("Running", true);
        }

        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        calc_damage = enemy.curPower + BSM.PerformList[0].chosenAttack.abilityDamage+ Random.Range(-5,5);
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

        if (stealth >0)
        {
            calc_damage = (calc_damage * 2);
            stealth--;
        }

        if (BSM.PerformList[0].chosenAttack.dotDuration>0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().dotDurations.Add(BSM.PerformList[0].chosenAttack.dotDuration);
            BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().dotDamages.Add(BSM.PerformList[0].chosenAttack.dotDamage);
        }

        if (BSM.PerformList[0].chosenAttack.weakened > 0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().weakened = BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().weakened + BSM.PerformList[0].chosenAttack.weakened;
        }

        if (BSM.PerformList[0].chosenAttack.vulnerable > 0)
        {
            BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().vulnerable = BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().weakened + BSM.PerformList[0].chosenAttack.vulnerable;
        }

        BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>().TakeDamage(calc_damage);
    }

    void Cleave()
    {
        calc_damage = enemy.curPower + BSM.PerformList[0].chosenAttack.abilityDamage + Random.Range(-5, 5);

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

        foreach (GameObject hero in BSM.HeroesInBattle)
        {
            if (enemy.tag != "DeadHero")
            {
                hero.GetComponent<HeroStateMachine>().TakeDamage(calc_damage);
            }
        }
    }

    void Heal()
    {
        float calc_healing = enemy.curPower + BSM.PerformList[0].chosenAttack.abilityDamage;

        if (weakened > 0)
        {
            calc_healing = (calc_healing / 2);
            weakened--;
        }

        if (strengthened > 0)
        {
            calc_healing = (calc_healing * 2);
            strengthened--;
        }
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().TakeHealing(calc_healing);
    }

    public void TakeHealing(float getHealingAmount)
    {
        enemy.curHP += getHealingAmount;
        if (enemy.curHP >= enemy.maxHP)
        {
            enemy.curHP = enemy.maxHP;
        }
    }

    void Buff()
    {
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().block += BSM.PerformList[0].chosenAttack.block;
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().taunt += BSM.PerformList[0].chosenAttack.taunt;
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().strengthened += BSM.PerformList[0].chosenAttack.strengthened;
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().stealth = BSM.PerformList[0].chosenAttack.stealth;
        BSM.PerformList[0].AttackersTarget.GetComponent<EnemyStateMachine>().enemy.curSpeed += BSM.PerformList[0].chosenAttack.hastened;
    }

    void Aura()
    {
        foreach (GameObject enemy in BSM.EnemiesInBattle)
        {
            if (enemy.tag != "DeadHero")
            {
                enemy.GetComponent<EnemyStateMachine>().strengthened += BSM.PerformList[0].chosenAttack.strengthened;
            }
        }
    }


    public void TakeDamage(float getDamageAmount)
    {
        animator.SetTrigger("Taking Hit");
        if (BSM.PerformList[0].chosenAttack.stun > 0)
        {
            cur_AP = (cur_AP - (BSM.PerformList[0].chosenAttack.stun * 100));
            enemy.curAP = (enemy.curAP - (BSM.PerformList[0].chosenAttack.stun * 100));
        }

        if (BSM.PerformList[0].chosenAttack.slowed > 0)
        {
            enemy.curSpeed -= BSM.PerformList[0].chosenAttack.slowed;
        }

        if (block > 0)
        {
            getDamageAmount = (getDamageAmount / 2);
            block--;
        }

        if (shielded > 0)
        {
            getDamageAmount = (getDamageAmount / 2);
            shielded--;
        }

        if (vulnerable>0)
        {
            getDamageAmount = (getDamageAmount * 1.2f);
            vulnerable--;
        }

        Debug.Log(this.gameObject.name + " has taken " + getDamageAmount + " damage!");

        enemy.curHP -= getDamageAmount;

        if (enemy.curHP <= 0)
        {
            enemy.curHP = 0;
            animator.SetBool("Dead", true);
            BSM.EnemiesAlive--;
            currentState = TurnState.DEAD;
            alive = false;
        }       
    }

    public void ApplyDotDamage()
    {
        if(dotDamages.Count>0)
        {
            int i = 0;
            foreach (int dot in dotDamages)
            {
                enemy.curHP -= dot;
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

    public void AdvantageEffects()
    {
        if (advantageState == "Hero Advantage")
        {
            enemy.curHP -= 10;
        }


        if (advantageState == "Hero Drop Advantage")
        {
            enemy.curHP -= 10;
            cur_AP = -50;
            enemy.curAP = -50;
        }

        if (advantageState == "Hero Dash Advantage")
        {
            enemy.curHP -= 10;
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
        if (BSM.PerformList[0].chosenAttack.abilityType == "Attack")
        {

            DoDamage();
        }
        else if (BSM.PerformList[0].chosenAttack.abilityType == "Buff")
        {
            if (BSM.PerformList[0].chosenAttack.abilityDamage > 0)
            {
                Heal();
            }
            Buff();
        }
        else if (BSM.PerformList[0].chosenAttack.abilityType == "Stance")
        {
            Buff();
        }
        else if (BSM.PerformList[0].chosenAttack.abilityType == "Aura")
        {
            Aura();
        }
        else if (BSM.PerformList[0].chosenAttack.abilityType == "Cleave")
        {

            Cleave();
        }
    }

    /*
    public void ActionAnimation()
    {

    }
    */

    public void ActionAnimation()
    {
        if (BSM.PerformList[0].chosenAttack.abilityType == "Attack")
        {
            animator.SetBool("Attacking", true);
            abilityVisualsAnimator.SetTrigger(BSM.PerformList[0].chosenAttack.abilityName);
        }

        if (BSM.PerformList[0].chosenAttack.abilityType == "Buff")
        {
            abilityVisualsAnimator.SetTrigger(BSM.PerformList[0].chosenAttack.abilityName);
        }

        if (BSM.PerformList[0].chosenAttack.abilityType == "Stance")
        {
            abilityVisualsAnimator.SetTrigger(BSM.PerformList[0].chosenAttack.abilityName);
        }

        if (BSM.PerformList[0].chosenAttack.abilityType == "Aura")
        {
            abilityVisualsAnimator.SetTrigger(BSM.PerformList[0].chosenAttack.abilityName);
        }

        if (BSM.PerformList[0].chosenAttack.abilityType == "Cleave")
        {
            abilityVisualsAnimator.SetTrigger(BSM.PerformList[0].chosenAttack.abilityName);
            animator.SetBool("Attacking", true);
        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Barrier")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "BattleCry")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Block")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Bludgeon")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Electrocute")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Empower")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Fireball")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Freeze")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Heal")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "MagicMissile")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "MarkTarget")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Slash")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Smash")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Smite")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Sneak")
        {
           // this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 225);
        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "Taunt")
        {

        }

        if (BSM.PerformList[0].chosenAttack.abilityName == "ArrowShot")
        {

        }
    }

    public void ApplyLevels()
    {
        enemy.curHP = enemy.lvl1HP + ((enemy.lvl1HP * 0.1f) * (enemy.level - 1));
        enemy.maxHP = enemy.lvl1HP + ((enemy.lvl1HP * 0.1f) * (enemy.level - 1));
        enemy.baseSpeed = enemy.lvl1Speed + ((enemy.lvl1Speed * 0.1f) * (enemy.level - 1));
        enemy.curSpeed = enemy.lvl1Speed + ((enemy.lvl1Speed * 0.1f) * (enemy.level - 1));
        enemy.basePower = enemy.lvl1Power + ((enemy.lvl1Power * 0.1f) * (enemy.level - 1));
        enemy.curPower = enemy.lvl1Power + ((enemy.lvl1Power * 0.1f) * (enemy.level - 1));
    }
}
