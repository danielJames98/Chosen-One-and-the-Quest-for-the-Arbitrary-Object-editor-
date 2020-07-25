using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelectButton : MonoBehaviour
{
    public GameObject HeroPrefab;
    private bool showSelector;

    public void SelectHero()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input2(HeroPrefab);//save input of enemy prefab
        HeroPrefab.transform.Find("Selector").gameObject.SetActive(false);
    }

    public void selectorOn()
    {
        HeroPrefab.transform.Find("Selector").gameObject.SetActive(true);
    }

    public void selectorOff()
    {
        HeroPrefab.transform.Find("Selector").gameObject.SetActive(false);
    }
}
