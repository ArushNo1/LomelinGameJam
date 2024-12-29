using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [SerializeField]
    [Range(0f, 100f)]
    public float health;
   
    public override void OnNetworkSpawn()
    {
        health = 100f;
    }

    private void HandleHealthChanged(float oldHealth, float newHealth)
    {
        Debug.Log($"Health changed from {oldHealth} to {newHealth}");
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void RespawnRpc(RpcParams rpcParams = default)
    {
        Debug.Log("Respawning player " + OwnerClientId);
        transform.position = new Vector3(0, 0, 0);
        health = 100f;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TakeDamageRpc(RpcParams rpcParams = default)
    {
        health -= 10;
    }

    public void OnCollisionEnter(Collision col)
    {
        if(!IsServer)
        {
            return;
        }

        if(col.gameObject.CompareTag("Snowball"))
        {
            ulong objOwner = col.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            if(objOwner != OwnerClientId)
            {
                Debug.Log("Snowball hit player" + OwnerClientId);
                TakeDamageRpc();
                if(health <= 0)
                {
                    Debug.Log("Player " + OwnerClientId + " died");
                    if(OwnerClientId == 0)
                    {
                        IncreaseScore2Rpc();
                    }
                    else 
                    {
                        IncreaseScore1Rpc();
                    }

                    //respawn player
                    RespawnRpc();
                }
                
                col.gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    
    [Rpc(SendTo.ClientsAndHost)]
    public void IncreaseScore1Rpc(RpcParams rpcParams = default)
    {
        GameManager.Instance.score1++;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void IncreaseScore2Rpc(RpcParams rpcParams = default)
    {
        GameManager.Instance.score2++;
    }
}