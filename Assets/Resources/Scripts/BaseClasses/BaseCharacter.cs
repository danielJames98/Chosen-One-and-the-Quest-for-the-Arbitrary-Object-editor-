using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter:MonoBehaviour
{
    public string characterName;
    public string spritePath;
    public string animatorPath;

    public bool leftFacingSprite;
    public float spriteScale;
    public float yOffset;
    public Vector2 forwardAbilitySource;
    public Vector2 overheadAbilitySource;
    public float maxHP;
    public float curHP;

    public float curAP;

    public float baseSpeed;
    public float curSpeed;

    public float basePower;
    public float curPower;

    public int level;

    public float lvl1HP;
    public float lvl1Speed;
    public float lvl1Power;

    public List<BaseAbility> attacks = new List<BaseAbility>();
}