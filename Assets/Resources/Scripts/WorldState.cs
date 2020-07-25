using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldState : MonoBehaviour
{
    public string currentLevel;
    public bool inCombat;

    public List<GameObject> CurrentUnitsInLevel = new List<GameObject>();
    public List<string> CurrentUnitsRef = new List<string>();
    public List<float> CurrentUnitPosX = new List<float>();
    public List<float> CurrentUnitPosY = new List<float>();

    public float pcPosX;
    public float pcPosY;

    public GameObject enemyInBattle;

    public bool heroVictory;

    public int enemyInBattleIndex;

    public string prefabPath;

    public bool levelPopulated;

    public string advantageState;

    public bool heroReady;
    public bool enemyReady;

    public AudioSource audioSource;

    public AudioClip plainsMusic;
    public AudioClip plainsBattleMusic;
    public AudioClip townMusic;
    public AudioClip dungeonMusic;
    public AudioClip dungeonBattleMusic;
    public AudioClip menuMusic;
    public AudioClip defeatMusic;
    public AudioClip victoryMusic;
    public AudioClip levelUpMusic;

    public bool battleEndMusicPlayed = true;
    
    public int playerXP;
    public int playerLevel;

    public Vector2 playerSpawn = new Vector2(0f,0f);

    public List<int> enemyLevels = new List<int>();

    public string newLevel;
    public bool newLevelLoading;
    public bool newLevelLoaded;

    public bool gameComplete = false;
    public GameObject QuestCompleteCanvas;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        audioSource.Play();        
    }

    private void Update()
    { 
        if ((SceneManager.GetActiveScene().name == "PlainsEmpty" ||  SceneManager.GetActiveScene().name == "DungeonEmpty") && levelPopulated == false)
        {
            PopulateLevel();
        }

        if (SceneManager.GetActiveScene().name == "Dungeon" && audioSource.clip!=dungeonMusic)
        {
            PlayZoneMusic();
        }

        if (heroReady==true && enemyReady==true && advantageState!=null)
        {
            SaveLevel();
        }

        if(newLevelLoading==true && newLevelLoaded==false && SceneManager.GetActiveScene().name==newLevel)
        {
            newLevelLoaded = true;
            newLevelLoading = false;
            StartNewLevel();
        }

        if(gameComplete==true && GameObject.Find("QuestCompleteCanvas")==null && GameObject.Find("QuestCompleteCanvas(Clone)")==null && SceneManager.GetActiveScene().name=="DungeonEmpty")
        {
            Instantiate(QuestCompleteCanvas);
        }

        if(Input.GetButtonDown("Cancel"))
        {
            SceneManager.LoadScene("MainMenu");
            Destroy(this.gameObject);
        }
    }

    public void PopulateLevel()
    {
        levelPopulated = true;
        if(heroVictory==true)
        {
            GameObject playerCharacter = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerCharacter"), new Vector2(pcPosX, pcPosY), this.transform.rotation) as GameObject;
            playerCharacter.name = "PlayerCharacter";
            playerCharacter.GetComponent<OverworldPlayerController>().curLevel = GameObject.Find("PlayerState").GetComponent<PlayerState>().Level;
            playerCharacter.GetComponent<OverworldPlayerController>().curXP = playerXP;
        }
        else
        {
            GameObject playerCharacter = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerCharacter"), playerSpawn, this.transform.rotation) as GameObject;
            playerCharacter.name = "PlayerCharacter";
            playerCharacter.GetComponent<OverworldPlayerController>().curLevel = GameObject.Find("PlayerState").GetComponent<PlayerState>().Level;
            playerCharacter.GetComponent<OverworldPlayerController>().curXP = playerXP;
        }
        
        

        if(heroVictory==true)
        {
            CurrentUnitsInLevel.RemoveAt(enemyInBattleIndex);
            CurrentUnitPosX.RemoveAt(enemyInBattleIndex);
            CurrentUnitPosY.RemoveAt(enemyInBattleIndex);
            CurrentUnitsRef.RemoveAt(enemyInBattleIndex);
            enemyLevels.RemoveAt(enemyInBattleIndex);
        }
        

        int i = 0;
        foreach (GameObject Unit in CurrentUnitsInLevel)
        {
            GameObject spawningUnit = Instantiate(Unit, new Vector2(CurrentUnitPosX[i], CurrentUnitPosY[i]), this.transform.rotation) as GameObject;
            spawningUnit.name = spawningUnit.GetComponent<OverworldBaseEnemy>().prefabName;
            spawningUnit.GetComponent<OverworldBaseEnemy>().curLevel = enemyLevels[i];
            i++;
        }

        CurrentUnitPosX.Clear();
        CurrentUnitPosY.Clear();
        CurrentUnitsRef.Clear();
        CurrentUnitsInLevel.Clear();
        enemyLevels.Clear();

        advantageState = null;
        currentLevel = SceneManager.GetActiveScene().name;
        PlayZoneMusic();

        if(heroVictory==true)
        {
            AddXP();
        }        

        if(currentLevel=="Dungeon"||currentLevel=="DungeonEmpty")
        {
            if (GameObject.Find("OverworldVampireLord")==null)
            {
                gameComplete = true;
            }
        }
    }

    public void SaveLevel()
    {
        currentLevel = SceneManager.GetActiveScene().name;
        if (currentLevel == "Plains" || currentLevel == "PlainsEmpty")
        {
            playerSpawn = new Vector2(-12, -3);
        }

        if (currentLevel == "Dungeon" || currentLevel == "DungeonEmpty")
        {
            playerSpawn = new Vector2(-4.65f, 2.59f);
        }

        if (GameObject.Find("PlayerCharacter") != null)
        {
            pcPosX = GameObject.Find("PlayerCharacter").GetComponent<Transform>().position.x;
            pcPosY = GameObject.Find("PlayerCharacter").GetComponent<Transform>().position.y;
            playerXP = GameObject.Find("PlayerCharacter").GetComponent<OverworldPlayerController>().curXP;
        }
        else if (GameObject.Find("PlayerCharacter")==null)
        {
            pcPosX = GameObject.Find("PlayerCharacter(Clone)").GetComponent<Transform>().position.x;
            pcPosY = GameObject.Find("PlayerCharacter(Clone)").GetComponent<Transform>().position.y;
        }



        foreach (GameObject Unit in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            prefabPath = "Prefabs/Enemies/Overworld/" + Unit.GetComponent<OverworldBaseEnemy>().prefab.ToString();
            Debug.Log(prefabPath);
            
            CurrentUnitsRef.Add(Unit.name);
            
            CurrentUnitsInLevel.Add(Unit.GetComponent<OverworldBaseEnemy>().prefab);
        }

        foreach (GameObject Unit in CurrentUnitsInLevel)
        {
            CurrentUnitPosX.Add(Unit.transform.position.x);
            CurrentUnitPosY.Add(Unit.transform.position.y);
            enemyLevels.Add(Unit.GetComponent<OverworldBaseEnemy>().curLevel);
        }

        enemyInBattleIndex = CurrentUnitsInLevel.IndexOf(enemyInBattle);
        Debug.Log(CurrentUnitsInLevel.IndexOf(enemyInBattle));

        if(currentLevel=="Plains"|| currentLevel=="PlainsEmpty")
        {
            SceneManager.LoadScene("PlainsCombat");
        } 
        else if (currentLevel == "Dungeon" || currentLevel == "DungeonEmpty")
        {
            SceneManager.LoadScene("DungeonCombat");
        }


        PlayBattleMusic();

        int i = 0;
        foreach (string Unit in CurrentUnitsRef)
        {
            prefabPath = "Prefabs/Enemies/Overworld/" + Unit;
            CurrentUnitsInLevel[i] = (Resources.Load<GameObject>(prefabPath));
            i++;
        }

        heroReady = false;
        enemyReady = false;
    }

    public void ReloadLevel()
    {
        levelPopulated = false;
        if (currentLevel=="Plains" || currentLevel == "PlainsEmpty")
        {
            SceneManager.LoadScene("PlainsEmpty");           
        }

        if (currentLevel=="Dungeon" || currentLevel == "DungeonEmpty")
        {
            SceneManager.LoadScene("DungeonEmpty");
        }     
    }

    public void LoadLevel(string levelToLoad, int xp, int level)
    {
        newLevel = levelToLoad;
        currentLevel = levelToLoad;
        if (currentLevel == "Plains" || currentLevel == "PlainsEmpty")
        {
            playerSpawn = new Vector2(-12, -3);
        }

        if (currentLevel == "Dungeon" || currentLevel == "DungeonEmpty")
        {
            playerSpawn = new Vector2(-4.65f, 2.59f);
        }
        playerLevel = level;
        playerXP = xp;
        SceneManager.LoadScene(levelToLoad);        
        newLevelLoading = true;
    }

    public void StartNewLevel()
    {
        GameObject playerCharacter = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerCharacter"), playerSpawn, this.transform.rotation) as GameObject;
        playerCharacter.name = "PlayerCharacter";
        playerCharacter.GetComponent<OverworldPlayerController>().curLevel = playerLevel;
        playerCharacter.GetComponent<OverworldPlayerController>().curXP = playerXP;
        GameObject.Find("PlayerCharacter").GetComponent<OverworldPlayerController>().LevelCheck();
        PlayZoneMusic();
    }

    public void PlayVictoryMusic()
    {
        audioSource.clip = null;
        audioSource.PlayOneShot(victoryMusic);
    }

    public void PlayDefeatMusic()
    {
        audioSource.clip = null;
        audioSource.PlayOneShot(defeatMusic);
    }

    public void PlayZoneMusic()
    {
        if (currentLevel == "Plains" || currentLevel == "PlainsEmpty")
        {
            audioSource.clip = plainsMusic;
        }
        else if (currentLevel == "Dungeon" || currentLevel == "DungeonEmpty")
        {
            audioSource.clip = dungeonMusic;
        }
        else if (currentLevel == "Town")
        {
            audioSource.clip = townMusic;
        }
        audioSource.Play();
    }

    public void PlayBattleMusic()
    {
        if (currentLevel == "Plains" || currentLevel == "PlainsEmpty")
        {
            audioSource.clip = plainsBattleMusic;
            
        }
        else if (currentLevel == "Dungeon" || currentLevel == "DungeonEmpty")
        {
            audioSource.clip = dungeonBattleMusic;
        }
        audioSource.Play();
    }

    public void AddXP()
    {        
        GameObject.Find("PlayerCharacter").GetComponent<OverworldPlayerController>().curXP += 50;
        GameObject.Find("PlayerCharacter").GetComponent<OverworldPlayerController>().LevelCheck();
    }
}
