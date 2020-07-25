using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooseArrow : BaseAbility
{
    public void Start()
    {
        abilityDamage = (Random.Range(15, 26));
    }

    public LooseArrow()
    {
        abilityName = "LooseArrow";
        abilityDescription = "Loose an Arrow towards your target, dealing damage from afar";
        abilityCost = 0;
        abilityType = "Attack";
    }
}
