using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash: BaseAbility
{
    public void Start()
    {
        abilityDamage = (Random.Range(15, 26));
    }

    public Slash()
    {
        abilityName = "Slash";
        abilityDescription = "Strike the target with your weapon, dealing moderate damage";
        abilityCost = 0;
        abilityType = "Attack";
    }
}
