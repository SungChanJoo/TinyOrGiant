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
    private PCPlayerController PCPlayer;
    private void Awake()
    {
        PCPlayer = transform.root.gameObject.GetComponent<PCPlayerController>();
    }

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
        
        if(PCPlayer != null)
        {
            PCPlayer.ToggleRagdoll(true);
            if (PCPlayer.IsGrab) return; 
        }
        currentDamageDelay = damageDelay;
        currentHp -= damage;
        Debug.Log("currentHp : " + currentHp);
        if(currentHp <= 0)
        {
            Die();
        }
    }
    
    [Command(requiresAuthority =false)]
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
