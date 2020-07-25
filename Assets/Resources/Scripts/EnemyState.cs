using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public BaseEnemy Enemy0;
    public BaseEnemy Enemy1;
    public BaseEnemy Enemy2;
    public BaseEnemy Enemy3;

    public int level;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Enemy0 = this.transform.Find("Enemy0").GetComponent<BaseEnemy>();
        Enemy1 = this.transform.Find("Enemy1").GetComponent<BaseEnemy>();
        Enemy2 = this.transform.Find("Enemy2").GetComponent<BaseEnemy>();
        Enemy3 = this.transform.Find("Enemy3").GetComponent<BaseEnemy>();
    }

}
