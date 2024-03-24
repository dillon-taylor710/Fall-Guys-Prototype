using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : NetworkBehaviour
{
    private Rigidbody rb;
    private Vector3 spawnPoint = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //if (!isServer) return;

        rb = GetComponent<Rigidbody>();
        rb.AddForce(-transform.forward * 10000, ForceMode.Impulse);
        spawnPoint = transform.position;
        //StartCoroutine(SpawnReset());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
            ResetBall();
    }
    IEnumerator SpawnReset()
    {
        while(true)
        {
            yield return new WaitUntil(() => rb.velocity.magnitude > 1.0f);
            yield return new WaitUntil(() => rb.velocity.magnitude < 1.0f);
            yield return new WaitForSeconds(3.0f);
            ResetBall();
        }
    }

    private void ResetBall()
    {
        rb.velocity = Vector3.zero;
        rb.MoveRotation(Quaternion.Euler(0, 0, 0));
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        //yield return new WaitForEndOfFrame();
        rb.MovePosition(spawnPoint);

        //yield return new WaitForEndOfFrame();
        rb.AddForce(-transform.forward * 10000, ForceMode.Impulse);
    }
}
