using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnerBehaviour : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.attachedRigidbody.velocity = Vector3.zero;
            other.attachedRigidbody.angularVelocity = Vector3.zero;
            other.transform.position = other.GetComponent<Player>().last_point + new Vector3(0, 1, 0);
        }
    }
}
