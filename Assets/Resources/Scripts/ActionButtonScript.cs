using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonScript : MonoBehaviour
{
    public BaseAbility ButtonAction;
    public GameObject descriptionBox;
    public Text descriptionBoxText;
    public string description;


    public void SelectAction()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input1(ButtonAction);
    }

    public void ShowDescription()
    {
        description = ButtonAction.abilityDescription;
        descriptionBox.SetActive(true);
        descriptionBoxText.text = description;
    }

    public void HideDescription()
    {
        descriptionBox.SetActive(false);
    }
}
