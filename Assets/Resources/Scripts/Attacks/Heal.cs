using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : BaseAbility
{
    // Start is called before the first frame update
    void Start()
    {
        abilityDamage = (Random.Range(5, 11));
    }

    public Heal()
    {
        abilityName = "Heal";
        abilityDescription = "Infuse the target with holy light, restoring health";
        abilityCost = 0;
        abilityType = "Buff";
    }

}