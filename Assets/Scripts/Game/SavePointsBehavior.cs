using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointsBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().last_point = other.transform.position;
        }
    }
}
