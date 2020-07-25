using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleStateMachine : MonoBehaviour
{

    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION
    }

    public PerformAction battleStates;
    public GameObject performer;

    public List<HandleTurn> PerformList = new List<HandleTurn>();

    public List<GameObject> HeroesInBattle = new List<GameObject>(4);
    public List<GameObject> EnemiesInBattle = new List<GameObject>(4);

    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE
    }

    public HeroGUI HeroInput;
    public HeroStateMachine HSM;
    public List<GameObject> HeroesToManage = new List<GameObject>();
    public HandleTurn HeroChoice;

    public GameObject enemyButton;
    public GameObject heroButton;
    public List<GameObject> TargetButtons = new List<GameObject>();
    public Transform TargetPanelSpacer;
    

    public GameObject ActionPanel;
    public GameObject TargetSelectPanel;

    public GameObject ActionButtonPrefab;
    public GameObject ActionButton1;
    public GameObject ActionButton2;
    public GameObject ActionButton3;
    public GameObject ActionButton4;

    public List<GameObject> ActionButtons = new List<GameObject>();
    public Transform ActionPanelSpacer;

    public Text EndingText;
    public GameObject EndBattleButton;
    public bool BattleOver;

    public PlayerState playerState;
    public EnemyState enemyState;
    private Transform HeroPanelSpacer;
    private Transform EnemyPanelSpacer;

    public GameObject CharacterBar;
    public GameObject CharacterBar0;
    public GameObject CharacterBar1;
    public GameObject CharacterBar2;
    public GameObject CharacterBar3;

    public GameObject EnemyBar0;
    public GameObject EnemyBar1;
    public GameObject EnemyBar2;
    public GameObject EnemyBar3;

    public bool CharacterBarsCreated;

    public bool battleStarted;
    public int HeroesAlive = 4;
    public int EnemiesAlive = 4;

    public string advantageState;

    // Start is called before the first frame update
    void Start()
    {
        playerState = GameObject.Find("PlayerState").GetComponent<PlayerState>();
        enemyState = GameObject.Find("EnemyState").GetComponent<EnemyState>();  
        battleStates = PerformAction.WAIT;
        HeroesInBattle.Add(GameObject.Find("Hero0"));
        HeroesInBattle.Add(GameObject.Find("Hero1"));
        HeroesInBattle.Add(GameObject.Find("Hero2"));
        HeroesInBattle.Add(GameObject.Find("Hero3"));
        EnemiesInBattle.Add(GameObject.Find("Enemy0"));
        EnemiesInBattle.Add(GameObject.Find("Enemy1"));
        EnemiesInBattle.Add(GameObject.Find("Enemy2"));
        EnemiesInBattle.Add(GameObject.Find("Enemy3"));
        SpawnUnits();
        HeroInput = HeroGUI.ACTIVATE;

        ActionPanel.SetActive(false);
        TargetSelectPanel.SetActive(false);
        //find spacer
        HeroPanelSpacer = GameObject.Find("HeroPanelSpacer").transform;
        EnemyPanelSpacer = GameObject.Find("EnemyPanelSpacer").transform;
        CreateCharacterBars();
        battleStarted = true;


        advantageState = GameObject.Find("WorldState").GetComponent<WorldState>().advantageState;
        foreach (GameObject unit in HeroesInBattle)
        {
            unit.GetComponent<HeroStateMachine>().advantageState = advantageState;
        }

        foreach(GameObject unit in EnemiesInBattle)
        {
            unit.GetComponent<EnemyStateMachine>().advantageState = advantageState;
        }

}

    // Update is called once per frame
    void Update()
    {
        switch (battleStates)
        {
            case(PerformAction.WAIT):
                if(PerformList.Count > 0)
                {
                    battleStates = PerformAction.TAKEACTION;
                }
            break;
            case(PerformAction.TAKEACTION):
                GameObject performer = PerformList[0].AttackersGameObject;
                if (PerformList[0].Type == "Enemy")
                {
                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();

                    ESM.Target = PerformList[0].AttackersTarget;
                    ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                }
                        
                if (PerformList[0].Type == "Hero")
                {

                    HSM = performer.GetComponent<HeroStateMachine>();
                    HSM.Target = PerformList[0].AttackersTarget;
                    HSM.currentState = HeroStateMachine.TurnState.ACTION;
                }

                foreach(GameObject unit in EnemiesInBattle)
                {
                    if (unit.tag!="DeadEnemy")
                    {
                        unit.GetComponent<EnemyStateMachine>().ApplyDotDamage();
                    }
                }

                foreach (GameObject unit in HeroesInBattle)
                {
                    if (unit.tag != "DeadHero")
                    {
                        unit.GetComponent<HeroStateMachine>().ApplyDotDamage();
                    }
                }

                battleStates = PerformAction.PERFORMACTION;
            break;
            case(PerformAction.PERFORMACTION):
                //idle
            break;
        }

        switch(HeroInput)
        {
            case (HeroGUI.ACTIVATE):
                if(HeroesToManage.Count>0 && PerformList.Count==0)
                {
                    HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    HeroChoice = new HandleTurn();

                    ActionPanel.SetActive(true);

                    //populate action buttons
                    CreateActionButtons();

                    HeroInput = HeroGUI.WAITING;
                }
                break;
            case (HeroGUI.WAITING):
                //idle
                break;
            case (HeroGUI.DONE):
                HeroInputDone();
                break;

        }

        if (EnemiesAlive == 0 && battleStarted==true && BattleOver==false)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().PlayVictoryMusic();
            EndingText.text = "Victory!";
            EndBattleButton.SetActive(true);
            ActionPanel.SetActive(false);
            TargetSelectPanel.SetActive(false);
            HSM = null;
            HeroesToManage.Clear();
            battleStates = PerformAction.WAIT;
            HeroInput = HeroGUI.WAITING;
            performer = null;
            //clean actionpanel
            foreach (GameObject ActionButton in ActionButtons)
            {
                Destroy(ActionButton);
            }
            ActionButtons.Clear();

            foreach (GameObject targetButton in TargetButtons)
            {
                Destroy(targetButton);
            }
            TargetButtons.Clear();

            GameObject.Find("WorldState").GetComponent<WorldState>().heroVictory = true;

            BattleOver = true;
        }

        if (HeroesAlive == 0 && battleStarted==true && BattleOver==false)
        {
            GameObject.Find("WorldState").GetComponent<WorldState>().PlayDefeatMusic();
            EndingText.text = "Defeat";
            EndBattleButton.SetActive(true);
            ActionPanel.SetActive(false);
            TargetSelectPanel.SetActive(false);
            HSM = null;
            HeroesToManage.Clear();
            battleStates = PerformAction.WAIT;
            HeroInput = HeroGUI.WAITING;
            performer = null;
            //clean actionpanel
            foreach (GameObject ActionButton in ActionButtons)
            {
                Destroy(ActionButton);
            }
            ActionButtons.Clear();

            foreach (GameObject targetButton in TargetButtons)
            {
                Destroy(targetButton);
            }
            TargetButtons.Clear();

            GameObject.Find("WorldState").GetComponent<WorldState>().heroVictory = false;
            BattleOver = true;
        }

        if(CharacterBarsCreated==true)
        {
            UpdateCharacterBars();
        }
    }

    public void CollectActions(HandleTurn input)
    {
        PerformList.Add(input);
    }

    void EnemyButtons()
    {
        foreach(GameObject enemy in EnemiesInBattle)
        {
            if (enemy.tag != "DeadEnemy" && (enemy.GetComponent<EnemyStateMachine>().taunt>0))
            {
                GameObject targetButton = Instantiate(enemyButton) as GameObject;
                EnemySelectButton button = targetButton.GetComponent<EnemySelectButton>();

                EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

                Text targetButtonText = targetButton.transform.Find("Text").gameObject.GetComponent<Text>();
                targetButtonText.text = cur_enemy.enemy.characterName;

                button.EnemyPrefab = enemy;

                TargetButtons.Add(targetButton);

                targetButton.transform.SetParent(TargetPanelSpacer, false);
            }

            if (TargetButtons.Count == 0)
            {
                foreach (GameObject enemyNoTaunt in EnemiesInBattle)
                {
                    if (enemyNoTaunt.tag != "DeadEnemy" && enemyNoTaunt.GetComponent<EnemyStateMachine>().stealth <1)
                    {
                        GameObject targetButton = Instantiate(enemyButton) as GameObject;
                        EnemySelectButton button = targetButton.GetComponent<EnemySelectButton>();

                        EnemyStateMachine cur_enemy = enemyNoTaunt.GetComponent<EnemyStateMachine>();

                        Text targetButtonText = targetButton.transform.Find("Text").gameObject.GetComponent<Text>();
                        targetButtonText.text = cur_enemy.enemy.characterName;

                        button.EnemyPrefab = enemyNoTaunt;

                        TargetButtons.Add(targetButton);

                        targetButton.transform.SetParent(TargetPanelSpacer, false);
                    }

                }

            }
         
        }
    }

    void HeroButtons()
    {
        foreach (GameObject hero in HeroesInBattle)
        {
            if (hero.tag != "DeadHero")
            {
                GameObject targetButton = Instantiate(heroButton) as GameObject;
                HeroSelectButton button = targetButton.GetComponent<HeroSelectButton>();

                HeroStateMachine cur_Hero = hero.GetComponent<HeroStateMachine>();

                Text targetButtonText = targetButton.transform.Find("Text").gameObject.GetComponent<Text>();
                targetButtonText.text = cur_Hero.hero.characterName;

                button.HeroPrefab = hero;

                TargetButtons.Add(targetButton);

                targetButton.transform.SetParent(TargetPanelSpacer, false);
            }
        }
    }

    public void Input1(BaseAbility ButtonAction)//attack button
    {
        HeroChoice.Attacker = HeroesToManage[0].name;
        HeroChoice.AttackersGameObject = HeroesToManage[0];
        HeroChoice.Type = "Hero";
        HeroChoice.chosenAttack = ButtonAction;
        ActionPanel.SetActive(false);
        TargetSelectPanel.SetActive(true);

        if (HeroChoice.chosenAttack.abilityType== "Attack")
        {
            EnemyButtons();
        }

        if (HeroChoice.chosenAttack.abilityType=="Buff")
        {
            HeroButtons();
        }

        if (HeroChoice.chosenAttack.abilityType=="Stance")
        {
            Input2(HeroChoice.AttackersGameObject);
        }

        if (HeroChoice.chosenAttack.abilityType == "Aura")
        {
            Input2(HeroChoice.AttackersGameObject);
        }

        if (HeroChoice.chosenAttack.abilityType == "Cleave")
        {
            Input2(EnemiesInBattle[0]);
        }
    }

    public void Input2(GameObject chosenTarget)//enemy selection
    {
        HeroChoice.AttackersTarget = chosenTarget;

        if (HeroChoice.chosenAttack.abilityType=="Attack" && HeroChoice.AttackersTarget.GetComponent<EnemyStateMachine>().taunt>0)
        {
            HeroChoice.AttackersTarget.GetComponent<HeroStateMachine>().taunt--;
        }

        HeroInput = HeroGUI.DONE;
    }

    public void HeroInputDone()
    {
        PerformList.Add(HeroChoice);
        TargetSelectPanel.SetActive(false);

        //clean actionpanel
        foreach(GameObject ActionButton in ActionButtons)
        {
            Destroy(ActionButton);
        }
        ActionButtons.Clear();

        foreach (GameObject targetButton in TargetButtons)
        {
            Destroy(targetButton);
        }
        TargetButtons.Clear();

        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HeroesToManage.RemoveAt(0);
        HeroInput = HeroGUI.ACTIVATE;
    }

    //create actionbuttons
    void CreateActionButtons()
    {
        ActionButton1 = Instantiate(ActionButtonPrefab) as GameObject;
        Text ActionButtonText1 = ActionButton1.transform.Find("Text").gameObject.GetComponent<Text>();
        ActionButtonText1.text = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0].abilityName;
        ActionButton1.GetComponent<ActionButtonScript>().ButtonAction = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0];
        BaseAbility ButtonAction1 = ActionButton1.GetComponent<ActionButtonScript>().ButtonAction;
        ButtonAction1 = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0];
        ActionButton1.transform.SetParent(ActionPanelSpacer, false);
        ActionButtons.Add(ActionButton1);

        ActionButton2 = Instantiate(ActionButtonPrefab) as GameObject;
        Text ActionButtonText2 = ActionButton2.transform.Find("Text").gameObject.GetComponent<Text>();
        ActionButtonText2.text = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[1].abilityName;
        ActionButton2.GetComponent<ActionButtonScript>().ButtonAction = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[1];
        BaseAbility ButtonAction2 = ActionButton1.GetComponent<ActionButtonScript>().ButtonAction;
        ButtonAction2 = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[1];
        ActionButton2.transform.SetParent(ActionPanelSpacer, false);
        ActionButtons.Add(ActionButton2);

        ActionButton3 = Instantiate(ActionButtonPrefab) as GameObject;
        Text ActionButtonText3 = ActionButton3.transform.Find("Text").gameObject.GetComponent<Text>();
        ActionButtonText3.text = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[2].abilityName;
        ActionButton3.GetComponent<ActionButtonScript>().ButtonAction = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[2];
        BaseAbility ButtonAction3 = ActionButton1.GetComponent<ActionButtonScript>().ButtonAction;
        ButtonAction3 = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[2];
        ActionButton3.transform.SetParent(ActionPanelSpacer, false);
        ActionButtons.Add(ActionButton3);

        ActionButton4 = Instantiate(ActionButtonPrefab) as GameObject;
        Text ActionButtonText4 = ActionButton4.transform.Find("Text").gameObject.GetComponent<Text>();
        ActionButtonText4.text = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[3].abilityName;
        ActionButton4.GetComponent<ActionButtonScript>().ButtonAction = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[3];
        BaseAbility ButtonAction4 = ActionButton1.GetComponent<ActionButtonScript>().ButtonAction;
        ButtonAction4 = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[3];
        ActionButton4.transform.SetParent(ActionPanelSpacer, false);
        ActionButtons.Add(ActionButton4);
    }

    public void EndBattle()
    {
        GameObject.Find("WorldState").GetComponent<WorldState>().ReloadLevel();
    }

    void SpawnUnits()
    {
        HeroesInBattle[0].GetComponent<BaseHero>().characterName = playerState.Hero0.GetComponent<BaseHero>().characterName;
        HeroesInBattle[0].GetComponent<BaseHero>().spritePath = playerState.Hero0.GetComponent<BaseHero>().spritePath;
        HeroesInBattle[0].GetComponent<BaseHero>().animatorPath = playerState.Hero0.GetComponent<BaseHero>().animatorPath;
        HeroesInBattle[0].GetComponent<BaseHero>().forwardAbilitySource = playerState.Hero0.GetComponent<BaseHero>().forwardAbilitySource;
        HeroesInBattle[0].GetComponent<BaseHero>().overheadAbilitySource = playerState.Hero0.GetComponent<BaseHero>().overheadAbilitySource;
        HeroesInBattle[0].GetComponent<BaseHero>().maxHP = playerState.Hero0.GetComponent<BaseHero>().maxHP;
        HeroesInBattle[0].GetComponent<BaseHero>().curHP = playerState.Hero0.GetComponent<BaseHero>().curHP;
        HeroesInBattle[0].GetComponent<BaseHero>().curAP = playerState.Hero0.GetComponent<BaseHero>().curAP;
        HeroesInBattle[0].GetComponent<BaseHero>().baseSpeed = playerState.Hero0.GetComponent<BaseHero>().baseSpeed;
        HeroesInBattle[0].GetComponent<BaseHero>().curSpeed = playerState.Hero0.GetComponent<BaseHero>().curSpeed;
        HeroesInBattle[0].GetComponent<BaseHero>().basePower = playerState.Hero0.GetComponent<BaseHero>().basePower;
        HeroesInBattle[0].GetComponent<BaseHero>().curPower = playerState.Hero0.GetComponent<BaseHero>().curPower;
        HeroesInBattle[0].GetComponent<BaseHero>().level = playerState.Hero0.GetComponent<BaseHero>().level;
        HeroesInBattle[0].GetComponent<BaseHero>().lvl1HP = playerState.Hero0.GetComponent<BaseHero>().lvl1HP;
        HeroesInBattle[0].GetComponent<BaseHero>().lvl1Speed = playerState.Hero0.GetComponent<BaseHero>().lvl1Speed;
        HeroesInBattle[0].GetComponent<BaseHero>().lvl1Power = playerState.Hero0.GetComponent<BaseHero>().lvl1Power;
        HeroesInBattle[0].GetComponent<BaseHero>().attacks[0] = playerState.Hero0.GetComponent<BaseHero>().attacks[0];
        HeroesInBattle[0].GetComponent<BaseHero>().attacks[1] = playerState.Hero0.GetComponent<BaseHero>().attacks[1];
        HeroesInBattle[0].GetComponent<BaseHero>().attacks[2] = playerState.Hero0.GetComponent<BaseHero>().attacks[2];
        HeroesInBattle[0].GetComponent<BaseHero>().attacks[3] = playerState.Hero0.GetComponent<BaseHero>().attacks[3];


        HeroesInBattle[1].GetComponent<BaseHero>().characterName = playerState.Hero1.GetComponent<BaseHero>().characterName;
        HeroesInBattle[1].GetComponent<BaseHero>().spritePath = playerState.Hero1.GetComponent<BaseHero>().spritePath;
        HeroesInBattle[1].GetComponent<BaseHero>().animatorPath = playerState.Hero1.GetComponent<BaseHero>().animatorPath;
        HeroesInBattle[1].GetComponent<BaseHero>().forwardAbilitySource = playerState.Hero1.GetComponent<BaseHero>().forwardAbilitySource;
        HeroesInBattle[1].GetComponent<BaseHero>().overheadAbilitySource = playerState.Hero1.GetComponent<BaseHero>().overheadAbilitySource;
        HeroesInBattle[1].GetComponent<BaseHero>().maxHP = playerState.Hero1.GetComponent<BaseHero>().maxHP;
        HeroesInBattle[1].GetComponent<BaseHero>().curHP = playerState.Hero1.GetComponent<BaseHero>().curHP;
        HeroesInBattle[1].GetComponent<BaseHero>().curAP = playerState.Hero1.GetComponent<BaseHero>().curAP;
        HeroesInBattle[1].GetComponent<BaseHero>().baseSpeed = playerState.Hero1.GetComponent<BaseHero>().baseSpeed;
        HeroesInBattle[1].GetComponent<BaseHero>().curSpeed = playerState.Hero1.GetComponent<BaseHero>().curSpeed;
        HeroesInBattle[1].GetComponent<BaseHero>().basePower = playerState.Hero1.GetComponent<BaseHero>().basePower;
        HeroesInBattle[1].GetComponent<BaseHero>().curPower = playerState.Hero1.GetComponent<BaseHero>().curPower;
        HeroesInBattle[1].GetComponent<BaseHero>().level = playerState.Hero1.GetComponent<BaseHero>().level;
        HeroesInBattle[1].GetComponent<BaseHero>().lvl1HP = playerState.Hero1.GetComponent<BaseHero>().lvl1HP;
        HeroesInBattle[1].GetComponent<BaseHero>().lvl1Speed = playerState.Hero1.GetComponent<BaseHero>().lvl1Speed;
        HeroesInBattle[1].GetComponent<BaseHero>().lvl1Power = playerState.Hero1.GetComponent<BaseHero>().lvl1Power;
        HeroesInBattle[1].GetComponent<BaseHero>().attacks[0] = playerState.Hero1.GetComponent<BaseHero>().attacks[0];
        HeroesInBattle[1].GetComponent<BaseHero>().attacks[1] = playerState.Hero1.GetComponent<BaseHero>().attacks[1];
        HeroesInBattle[1].GetComponent<BaseHero>().attacks[2] = playerState.Hero1.GetComponent<BaseHero>().attacks[2];
        HeroesInBattle[1].GetComponent<BaseHero>().attacks[3] = playerState.Hero1.GetComponent<BaseHero>().attacks[3];

        HeroesInBattle[2].GetComponent<BaseHero>().characterName = playerState.Hero2.GetComponent<BaseHero>().characterName;
        HeroesInBattle[2].GetComponent<BaseHero>().spritePath = playerState.Hero2.GetComponent<BaseHero>().spritePath;
        HeroesInBattle[2].GetComponent<BaseHero>().animatorPath = playerState.Hero2.GetComponent<BaseHero>().animatorPath;
        HeroesInBattle[2].GetComponent<BaseHero>().forwardAbilitySource = playerState.Hero2.GetComponent<BaseHero>().forwardAbilitySource;
        HeroesInBattle[2].GetComponent<BaseHero>().overheadAbilitySource = playerState.Hero2.GetComponent<BaseHero>().overheadAbilitySource;
        HeroesInBattle[2].GetComponent<BaseHero>().maxHP = playerState.Hero2.GetComponent<BaseHero>().maxHP;
        HeroesInBattle[2].GetComponent<BaseHero>().curHP = playerState.Hero2.GetComponent<BaseHero>().curHP;
        HeroesInBattle[2].GetComponent<BaseHero>().curAP = playerState.Hero2.GetComponent<BaseHero>().curAP;
        HeroesInBattle[2].GetComponent<BaseHero>().baseSpeed = playerState.Hero2.GetComponent<BaseHero>().baseSpeed;
        HeroesInBattle[2].GetComponent<BaseHero>().curSpeed = playerState.Hero2.GetComponent<BaseHero>().curSpeed;
        HeroesInBattle[2].GetComponent<BaseHero>().basePower = playerState.Hero2.GetComponent<BaseHero>().basePower;
        HeroesInBattle[2].GetComponent<BaseHero>().curPower = playerState.Hero2.GetComponent<BaseHero>().curPower;
        HeroesInBattle[2].GetComponent<BaseHero>().level = playerState.Hero2.GetComponent<BaseHero>().level;
        HeroesInBattle[2].GetComponent<BaseHero>().lvl1HP = playerState.Hero2.GetComponent<BaseHero>().lvl1HP;
        HeroesInBattle[2].GetComponent<BaseHero>().lvl1Speed = playerState.Hero2.GetComponent<BaseHero>().lvl1Speed;
        HeroesInBattle[2].GetComponent<BaseHero>().lvl1Power = playerState.Hero2.GetComponent<BaseHero>().lvl1Power;
        HeroesInBattle[2].GetComponent<BaseHero>().attacks[0] = playerState.Hero2.GetComponent<BaseHero>().attacks[0];
        HeroesInBattle[2].GetComponent<BaseHero>().attacks[1] = playerState.Hero2.GetComponent<BaseHero>().attacks[1];
        HeroesInBattle[2].GetComponent<BaseHero>().attacks[2] = playerState.Hero2.GetComponent<BaseHero>().attacks[2];
        HeroesInBattle[2].GetComponent<BaseHero>().attacks[3] = playerState.Hero2.GetComponent<BaseHero>().attacks[3];

        HeroesInBattle[3].GetComponent<BaseHero>().characterName = playerState.Hero3.GetComponent<BaseHero>().characterName;
        HeroesInBattle[3].GetComponent<BaseHero>().spritePath = playerState.Hero3.GetComponent<BaseHero>().spritePath;
        HeroesInBattle[3].GetComponent<BaseHero>().animatorPath = playerState.Hero3.GetComponent<BaseHero>().animatorPath;
        HeroesInBattle[3].GetComponent<BaseHero>().forwardAbilitySource = playerState.Hero3.GetComponent<BaseHero>().forwardAbilitySource;
        HeroesInBattle[3].GetComponent<BaseHero>().overheadAbilitySource = playerState.Hero3.GetComponent<BaseHero>().overheadAbilitySource;
        HeroesInBattle[3].GetComponent<BaseHero>().maxHP = playerState.Hero3.GetComponent<BaseHero>().maxHP;
        HeroesInBattle[3].GetComponent<BaseHero>().curHP = playerState.Hero3.GetComponent<BaseHero>().curHP;
        HeroesInBattle[3].GetComponent<BaseHero>().curAP = playerState.Hero3.GetComponent<BaseHero>().curAP;
        HeroesInBattle[3].GetComponent<BaseHero>().baseSpeed = playerState.Hero3.GetComponent<BaseHero>().baseSpeed;
        HeroesInBattle[3].GetComponent<BaseHero>().curSpeed = playerState.Hero3.GetComponent<BaseHero>().curSpeed;
        HeroesInBattle[3].GetComponent<BaseHero>().basePower = playerState.Hero3.GetComponent<BaseHero>().basePower;
        HeroesInBattle[3].GetComponent<BaseHero>().curPower = playerState.Hero3.GetComponent<BaseHero>().curPower;
        HeroesInBattle[3].GetComponent<BaseHero>().level = playerState.Hero3.GetComponent<BaseHero>().level;
        HeroesInBattle[3].GetComponent<BaseHero>().lvl1HP = playerState.Hero3.GetComponent<BaseHero>().lvl1HP;
        HeroesInBattle[3].GetComponent<BaseHero>().lvl1Speed = playerState.Hero3.GetComponent<BaseHero>().lvl1Speed;
        HeroesInBattle[3].GetComponent<BaseHero>().lvl1Power = playerState.Hero3.GetComponent<BaseHero>().lvl1Power;
        HeroesInBattle[3].GetComponent<BaseHero>().attacks[0] = playerState.Hero3.GetComponent<BaseHero>().attacks[0];
        HeroesInBattle[3].GetComponent<BaseHero>().attacks[1] = playerState.Hero3.GetComponent<BaseHero>().attacks[1];
        HeroesInBattle[3].GetComponent<BaseHero>().attacks[2] = playerState.Hero3.GetComponent<BaseHero>().attacks[2];
        HeroesInBattle[3].GetComponent<BaseHero>().attacks[3] = playerState.Hero3.GetComponent<BaseHero>().attacks[3];

        EnemiesInBattle[0].GetComponent<BaseEnemy>().characterName = enemyState.Enemy0.GetComponent<BaseEnemy>().characterName;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().spritePath = enemyState.Enemy0.GetComponent<BaseEnemy>().spritePath;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().animatorPath = enemyState.Enemy0.GetComponent<BaseEnemy>().animatorPath;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().leftFacingSprite = enemyState.Enemy0.GetComponent<BaseEnemy>().leftFacingSprite;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().spriteScale = enemyState.Enemy0.GetComponent<BaseEnemy>().spriteScale;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().yOffset = enemyState.Enemy0.GetComponent<BaseEnemy>().yOffset;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().forwardAbilitySource = enemyState.Enemy0.GetComponent<BaseEnemy>().forwardAbilitySource;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().overheadAbilitySource = enemyState.Enemy0.GetComponent<BaseEnemy>().overheadAbilitySource;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().maxHP = enemyState.Enemy0.GetComponent<BaseEnemy>().maxHP;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().curHP = enemyState.Enemy0.GetComponent<BaseEnemy>().curHP;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().curAP = enemyState.Enemy0.GetComponent<BaseEnemy>().curAP;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().baseSpeed = enemyState.Enemy0.GetComponent<BaseEnemy>().baseSpeed;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().curSpeed = enemyState.Enemy0.GetComponent<BaseEnemy>().curSpeed;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().basePower = enemyState.Enemy0.GetComponent<BaseEnemy>().basePower;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().curPower = enemyState.Enemy0.GetComponent<BaseEnemy>().curPower;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().level = enemyState.Enemy0.GetComponent<BaseEnemy>().level;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().lvl1HP = enemyState.Enemy0.GetComponent<BaseEnemy>().lvl1HP;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().lvl1Speed = enemyState.Enemy0.GetComponent<BaseEnemy>().lvl1Speed;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().lvl1Power = enemyState.Enemy0.GetComponent<BaseEnemy>().lvl1Power;
        EnemiesInBattle[0].GetComponent<BaseEnemy>().attacks[0] = enemyState.Enemy0.GetComponent<BaseEnemy>().attacks[0];
        EnemiesInBattle[0].GetComponent<BaseEnemy>().attacks[1] = enemyState.Enemy0.GetComponent<BaseEnemy>().attacks[1];
        EnemiesInBattle[0].GetComponent<BaseEnemy>().attacks[2] = enemyState.Enemy0.GetComponent<BaseEnemy>().attacks[2];
        EnemiesInBattle[0].GetComponent<BaseEnemy>().attacks[3] = enemyState.Enemy0.GetComponent<BaseEnemy>().attacks[3];

        EnemiesInBattle[1].GetComponent<BaseEnemy>().characterName = enemyState.Enemy1.GetComponent<BaseEnemy>().characterName;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().spritePath = enemyState.Enemy1.GetComponent<BaseEnemy>().spritePath;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().animatorPath = enemyState.Enemy1.GetComponent<BaseEnemy>().animatorPath;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().leftFacingSprite = enemyState.Enemy1.GetComponent<BaseEnemy>().leftFacingSprite;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().spriteScale = enemyState.Enemy1.GetComponent<BaseEnemy>().spriteScale;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().yOffset = enemyState.Enemy1.GetComponent<BaseEnemy>().yOffset;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().forwardAbilitySource = enemyState.Enemy1.GetComponent<BaseEnemy>().forwardAbilitySource;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().overheadAbilitySource = enemyState.Enemy1.GetComponent<BaseEnemy>().overheadAbilitySource;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().maxHP = enemyState.Enemy1.GetComponent<BaseEnemy>().maxHP;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().curHP = enemyState.Enemy1.GetComponent<BaseEnemy>().curHP;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().curAP = enemyState.Enemy1.GetComponent<BaseEnemy>().curAP;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().baseSpeed = enemyState.Enemy1.GetComponent<BaseEnemy>().baseSpeed;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().curSpeed = enemyState.Enemy1.GetComponent<BaseEnemy>().curSpeed;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().basePower = enemyState.Enemy1.GetComponent<BaseEnemy>().basePower;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().curPower = enemyState.Enemy1.GetComponent<BaseEnemy>().curPower;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().level = enemyState.Enemy1.GetComponent<BaseEnemy>().level;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().lvl1HP = enemyState.Enemy1.GetComponent<BaseEnemy>().lvl1HP;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().lvl1Speed = enemyState.Enemy1.GetComponent<BaseEnemy>().lvl1Speed;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().lvl1Power = enemyState.Enemy1.GetComponent<BaseEnemy>().lvl1Power;
        EnemiesInBattle[1].GetComponent<BaseEnemy>().attacks[0] = enemyState.Enemy1.GetComponent<BaseEnemy>().attacks[0];
        EnemiesInBattle[1].GetComponent<BaseEnemy>().attacks[1] = enemyState.Enemy1.GetComponent<BaseEnemy>().attacks[1];
        EnemiesInBattle[1].GetComponent<BaseEnemy>().attacks[2] = enemyState.Enemy1.GetComponent<BaseEnemy>().attacks[2];
        EnemiesInBattle[1].GetComponent<BaseEnemy>().attacks[3] = enemyState.Enemy1.GetComponent<BaseEnemy>().attacks[3];

        EnemiesInBattle[2].GetComponent<BaseEnemy>().characterName = enemyState.Enemy2.GetComponent<BaseEnemy>().characterName;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().spritePath = enemyState.Enemy2.GetComponent<BaseEnemy>().spritePath;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().animatorPath = enemyState.Enemy2.GetComponent<BaseEnemy>().animatorPath;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().leftFacingSprite = enemyState.Enemy2.GetComponent<BaseEnemy>().leftFacingSprite;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().spriteScale = enemyState.Enemy2.GetComponent<BaseEnemy>().spriteScale;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().yOffset = enemyState.Enemy2.GetComponent<BaseEnemy>().yOffset;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().forwardAbilitySource = enemyState.Enemy2.GetComponent<BaseEnemy>().forwardAbilitySource;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().overheadAbilitySource = enemyState.Enemy2.GetComponent<BaseEnemy>().overheadAbilitySource;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().maxHP = enemyState.Enemy2.GetComponent<BaseEnemy>().maxHP;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().curHP = enemyState.Enemy2.GetComponent<BaseEnemy>().curHP;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().curAP = enemyState.Enemy2.GetComponent<BaseEnemy>().curAP;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().baseSpeed = enemyState.Enemy2.GetComponent<BaseEnemy>().baseSpeed;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().curSpeed = enemyState.Enemy2.GetComponent<BaseEnemy>().curSpeed;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().basePower = enemyState.Enemy2.GetComponent<BaseEnemy>().basePower;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().curPower = enemyState.Enemy2.GetComponent<BaseEnemy>().curPower;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().level = enemyState.Enemy2.GetComponent<BaseEnemy>().level;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().lvl1HP = enemyState.Enemy2.GetComponent<BaseEnemy>().lvl1HP;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().lvl1Speed = enemyState.Enemy2.GetComponent<BaseEnemy>().lvl1Speed;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().lvl1Power = enemyState.Enemy2.GetComponent<BaseEnemy>().lvl1Power;
        EnemiesInBattle[2].GetComponent<BaseEnemy>().attacks[0] = enemyState.Enemy2.GetComponent<BaseEnemy>().attacks[0];
        EnemiesInBattle[2].GetComponent<BaseEnemy>().attacks[1] = enemyState.Enemy2.GetComponent<BaseEnemy>().attacks[1];
        EnemiesInBattle[2].GetComponent<BaseEnemy>().attacks[2] = enemyState.Enemy2.GetComponent<BaseEnemy>().attacks[2];
        EnemiesInBattle[2].GetComponent<BaseEnemy>().attacks[3] = enemyState.Enemy2.GetComponent<BaseEnemy>().attacks[3];

        EnemiesInBattle[3].GetComponent<BaseEnemy>().characterName = enemyState.Enemy3.GetComponent<BaseEnemy>().characterName;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().spritePath = enemyState.Enemy3.GetComponent<BaseEnemy>().spritePath;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().animatorPath = enemyState.Enemy3.GetComponent<BaseEnemy>().animatorPath;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().leftFacingSprite = enemyState.Enemy3.GetComponent<BaseEnemy>().leftFacingSprite;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().spriteScale = enemyState.Enemy3.GetComponent<BaseEnemy>().spriteScale;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().yOffset = enemyState.Enemy3.GetComponent<BaseEnemy>().yOffset;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().forwardAbilitySource = enemyState.Enemy3.GetComponent<BaseEnemy>().forwardAbilitySource;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().overheadAbilitySource = enemyState.Enemy3.GetComponent<BaseEnemy>().overheadAbilitySource;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().maxHP = enemyState.Enemy3.GetComponent<BaseEnemy>().maxHP;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().curHP = enemyState.Enemy3.GetComponent<BaseEnemy>().curHP;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().curAP = enemyState.Enemy3.GetComponent<BaseEnemy>().curAP;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().baseSpeed = enemyState.Enemy3.GetComponent<BaseEnemy>().baseSpeed;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().curSpeed = enemyState.Enemy3.GetComponent<BaseEnemy>().curSpeed;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().basePower = enemyState.Enemy3.GetComponent<BaseEnemy>().basePower;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().curPower = enemyState.Enemy3.GetComponent<BaseEnemy>().curPower;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().level = enemyState.Enemy3.GetComponent<BaseEnemy>().level;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().lvl1HP = enemyState.Enemy3.GetComponent<BaseEnemy>().lvl1HP;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().lvl1Speed = enemyState.Enemy3.GetComponent<BaseEnemy>().lvl1Speed;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().lvl1Power = enemyState.Enemy3.GetComponent<BaseEnemy>().lvl1Power;
        EnemiesInBattle[3].GetComponent<BaseEnemy>().attacks[0] = enemyState.Enemy3.GetComponent<BaseEnemy>().attacks[0];
        EnemiesInBattle[3].GetComponent<BaseEnemy>().attacks[1] = enemyState.Enemy3.GetComponent<BaseEnemy>().attacks[1];
        EnemiesInBattle[3].GetComponent<BaseEnemy>().attacks[2] = enemyState.Enemy3.GetComponent<BaseEnemy>().attacks[2];
        EnemiesInBattle[3].GetComponent<BaseEnemy>().attacks[3] = enemyState.Enemy3.GetComponent<BaseEnemy>().attacks[3];
    }

    void CreateCharacterBars()
    {
        CharacterBar0 = Instantiate(CharacterBar) as GameObject;
        CharacterBar0.GetComponent<CharacterBarStats>().CharacterName.text = HeroesInBattle[0].GetComponent<BaseHero>().characterName;
        CharacterBar0.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[0].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[0].GetComponent<BaseHero>().maxHP;
        CharacterBar0.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[0].GetComponent<BaseHero>().curAP;
        CharacterBar0.transform.SetParent(HeroPanelSpacer, false);

        CharacterBar1 = Instantiate(CharacterBar) as GameObject;
        CharacterBar1.GetComponent<CharacterBarStats>().CharacterName.text = HeroesInBattle[1].GetComponent<BaseHero>().characterName;
        CharacterBar1.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[1].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[1].GetComponent<BaseHero>().maxHP;
        CharacterBar1.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[1].GetComponent<BaseHero>().curAP;
        CharacterBar1.transform.SetParent(HeroPanelSpacer, false);

        CharacterBar2 = Instantiate(CharacterBar) as GameObject;
        CharacterBar2.GetComponent<CharacterBarStats>().CharacterName.text = HeroesInBattle[2].GetComponent<BaseHero>().characterName;
        CharacterBar2.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[2].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[2].GetComponent<BaseHero>().maxHP;
        CharacterBar2.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[2].GetComponent<BaseHero>().curAP;
        CharacterBar2.transform.SetParent(HeroPanelSpacer, false);

        CharacterBar3 = Instantiate(CharacterBar) as GameObject;
        CharacterBar3.GetComponent<CharacterBarStats>().CharacterName.text = HeroesInBattle[3].GetComponent<BaseHero>().characterName;
        CharacterBar3.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[3].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[3].GetComponent<BaseHero>().maxHP;
        CharacterBar3.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[3].GetComponent<BaseHero>().curAP;
        CharacterBar3.transform.SetParent(HeroPanelSpacer, false);

        EnemyBar0 = Instantiate(CharacterBar) as GameObject;
        EnemyBar0.GetComponent<CharacterBarStats>().CharacterName.text = EnemiesInBattle[0].GetComponent<BaseEnemy>().characterName;
        EnemyBar0.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[0].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[0].GetComponent<BaseEnemy>().maxHP;
        EnemyBar0.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[0].GetComponent<BaseEnemy>().curAP;
        EnemyBar0.transform.SetParent(EnemyPanelSpacer, false);

        EnemyBar1 = Instantiate(CharacterBar) as GameObject;
        EnemyBar1.GetComponent<CharacterBarStats>().CharacterName.text = EnemiesInBattle[1].GetComponent<BaseEnemy>().characterName;
        EnemyBar1.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[1].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[1].GetComponent<BaseEnemy>().maxHP;
        EnemyBar1.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[1].GetComponent<BaseEnemy>().curAP;
        EnemyBar1.transform.SetParent(EnemyPanelSpacer, false);

        EnemyBar2 = Instantiate(CharacterBar) as GameObject;
        EnemyBar2.GetComponent<CharacterBarStats>().CharacterName.text = EnemiesInBattle[2].GetComponent<BaseEnemy>().characterName;
        EnemyBar2.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[2].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[2].GetComponent<BaseEnemy>().maxHP;
        EnemyBar2.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[2].GetComponent<BaseEnemy>().curAP;
        EnemyBar2.transform.SetParent(EnemyPanelSpacer, false);

        EnemyBar3 = Instantiate(CharacterBar) as GameObject;
        EnemyBar3.GetComponent<CharacterBarStats>().CharacterName.text = EnemiesInBattle[3].GetComponent<BaseEnemy>().characterName;
        EnemyBar3.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[3].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[3].GetComponent<BaseEnemy>().maxHP;
        EnemyBar3.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[3].GetComponent<BaseEnemy>().curAP;
        EnemyBar3.transform.SetParent(EnemyPanelSpacer, false);

        CharacterBarsCreated = true;
    }

    void UpdateCharacterBars()
    {
        if(HeroesInBattle[0].tag != "DeadHero")
        {
            CharacterBar0.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[0].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[0].GetComponent<BaseHero>().maxHP;
            CharacterBar0.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[0].GetComponent<BaseHero>().curAP;
        }

        if (HeroesInBattle[1].tag != "DeadHero")
        {
            CharacterBar1.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[1].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[1].GetComponent<BaseHero>().maxHP;
            CharacterBar1.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[1].GetComponent<BaseHero>().curAP;
        }

        if (HeroesInBattle[2].tag != "DeadHero")
        {
            CharacterBar2.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[2].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[2].GetComponent<BaseHero>().maxHP;
            CharacterBar2.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[2].GetComponent<BaseHero>().curAP;
        }
            

        if (HeroesInBattle[3].tag != "DeadHero")
        {
            CharacterBar3.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + HeroesInBattle[3].GetComponent<BaseHero>().curHP + "/" + HeroesInBattle[3].GetComponent<BaseHero>().maxHP;
            CharacterBar3.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + HeroesInBattle[3].GetComponent<BaseHero>().curAP;
        }
            

        if (EnemiesInBattle[0].tag != "DeadEnemy")
        {
            EnemyBar0.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[0].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[0].GetComponent<BaseEnemy>().maxHP;
            EnemyBar0.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[0].GetComponent<BaseEnemy>().curAP;
        }

        if (EnemiesInBattle[1].tag != "DeadEnemy")
        {
            EnemyBar1.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[1].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[1].GetComponent<BaseEnemy>().maxHP;
            EnemyBar1.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[1].GetComponent<BaseEnemy>().curAP;
        }
            

        if (EnemiesInBattle[2].tag != "DeadEnemy")
        {
            EnemyBar2.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[2].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[2].GetComponent<BaseEnemy>().maxHP;
            EnemyBar2.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[2].GetComponent<BaseEnemy>().curAP;
        }
            

        if (EnemiesInBattle[3].tag != "DeadEnemy")
        {
            EnemyBar3.GetComponent<CharacterBarStats>().CharacterHP.text = "HP: " + EnemiesInBattle[3].GetComponent<BaseEnemy>().curHP + "/" + EnemiesInBattle[3].GetComponent<BaseEnemy>().maxHP;
            EnemyBar3.GetComponent<CharacterBarStats>().CharacterAP.text = "AP: " + EnemiesInBattle[3].GetComponent<BaseEnemy>().curAP;
        }
            
    }
}
