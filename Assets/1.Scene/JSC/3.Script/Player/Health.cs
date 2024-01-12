using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Health : NetworkBehaviour
{
    public float MaxHp = 3f;
    public float currentHp;
    public bool IsDead = false;

    private void Awake()
    {
        currentHp = MaxHp;
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log("currentHp : " + currentHp);
        if(currentHp <= 0)
        {
            Die();
        }
    }
    [Command]
    public void CmdTakeDamage(float damage)
    {
        RpcTakeDamage(damage);
    }
    [ClientRpc]
    public void RpcTakeDamage(float damage)
    {
        TakeDamage(damage);
    }
    public virtual void Die()
    {
        if(!IsDead)
            GameManager.Instance.ViewWinnerUI(GameManager.Instance.playerType);
        IsDead = true;
        //transform.root.gameObject.SetActive(false);
    }
}
