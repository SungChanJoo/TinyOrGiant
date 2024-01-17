using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Health : NetworkBehaviour
{
    public float MaxHp = 3f;
    public float currentHp;
    public bool IsDead = false;

    public float damageDelay = 0f;
    private float currentDamageDelay = 0f;

	private void Start()
	{
        currentHp = MaxHp;
        currentDamageDelay = damageDelay;
    }

	private void Update()
	{
		if (currentDamageDelay > 0) currentDamageDelay = Mathf.Max(0, currentDamageDelay - Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (currentDamageDelay != 0 || currentHp == 0) return;

        currentDamageDelay = damageDelay;
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
        
        if(gameObject.CompareTag("Player"))
        {
            GameManager.Instance.ViewWinnerUI(PlayerType.PC);
        }
        else
        {
            GameManager.Instance.ViewWinnerUI(PlayerType.VR);
        }
        IsDead = true;
        //transform.root.gameObject.SetActive(false);
    }
}
