using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalBehaviour : NetworkBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Player>().SetVictory(true);
            GameManager.singleton.RpcEndGame(other.GetComponent<NetworkIdentity>());
        }
    }

}