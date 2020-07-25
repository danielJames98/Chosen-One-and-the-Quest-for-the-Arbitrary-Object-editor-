using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smite: BaseAbility
{
    public void Start()
    {
        abilityDamage = (Random.Range(15, 26));
    }

    public Smite()
    {
        abilityName = "Smite";
        abilityDescription = "smite the target with holy energy";
        abilityCost = 0;
        abilityType = "Attack";
    }
}
