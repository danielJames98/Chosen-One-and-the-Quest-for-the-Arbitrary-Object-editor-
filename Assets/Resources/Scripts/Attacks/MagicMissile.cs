using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissile : BaseAbility
{
    public void Start()
    {
        abilityDamage = (Random.Range(15, 26));
    }

    public MagicMissile()
    {
        abilityName = "Magic Missile";
        abilityDescription = "Fire a blast of magical energy at the taret, dealing damage";
        abilityCost = 0;
        abilityType = "Attack";
    }
}
