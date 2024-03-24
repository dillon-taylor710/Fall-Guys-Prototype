using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AXIS
{
    X,
    Y,
    Z
}
public class BumperController : NetworkBehaviour
{
    public AXIS axis;
    public float min_value;
    public float max_value;
    public float speed;

    private Vector3 value;
    private int direction = 1;

    void Start()
    {
        if (axis == AXIS.X)
            value = transform.right;
        if (axis == AXIS.Y)
            value = transform.up;
        if (axis == AXIS.Z)
            value = transform.forward;
    }
    // Update is called once per frame
    void Update()
    {
        if (isClientOnly)
            return;

        transform.localPosition = transform.localPosition + value * direction * speed * Time.deltaTime;
        if (axis == AXIS.X)
        {
            if (transform.localPosition.x > max_value)
            {
                transform.localPosition = new Vector3(max_value, transform.localPosition.y, transform.localPosition.z);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = -1;
            }
            else if (transform.localPosition.x < min_value)
            {
                transform.localPosition = new Vector3(min_value, transform.localPosition.y, transform.localPosition.z);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = 1;
            }
        }
        else if (axis == AXIS.Y)
        {
            if (transform.localPosition.y > max_value)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, max_value, transform.localPosition.z);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = -1;
            }
            else if (transform.localPosition.y < min_value)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, min_value, transform.localPosition.z);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = 1;
            }
        }
        else if (axis == AXIS.Z)
        {
            if (transform.localPosition.z > max_value)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, max_value);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = -1;
            }
            else if (transform.localPosition.z < min_value)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, min_value);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;
                direction = 1;
            }
        }
    }
}
