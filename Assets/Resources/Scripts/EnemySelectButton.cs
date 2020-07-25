using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelectButton : MonoBehaviour
{
    public GameObject EnemyPrefab;
    private bool showSelector;

    public void SelectEnemy()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input2(EnemyPrefab);//save input of enemy prefab
        EnemyPrefab.transform.Find("Selector").gameObject.SetActive(false);
    }

    public void selectorOn()
    {
            EnemyPrefab.transform.Find("Selector").gameObject.SetActive(true);
    }

    public void selectorOff()
    {
            EnemyPrefab.transform.Find("Selector").gameObject.SetActive(false);
    }
}
