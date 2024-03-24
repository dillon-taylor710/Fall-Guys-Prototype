using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBridgeController : NetworkBehaviour
{
    public AXIS axis;
    public float min_value;
    public float max_value;
    public float speed;
    public float delay_time;

    private Vector3 value;
    private float angle = 0;
    private Vector3 old_angle;
    private int direction = 1;
    private float start_time = 0f;

    void Start()
    {
        angle = min_value;
        if (axis == AXIS.X)
        {
            value = new Vector3(1, 0, 0);
            old_angle = new Vector3(0, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        }
        if (axis == AXIS.Y)
        {
            value = new Vector3(0, 1, 0);
            old_angle = new Vector3(transform.localRotation.eulerAngles.x, 0, transform.localRotation.eulerAngles.z);
        }
        if (axis == AXIS.Z)
        {
            value = new Vector3(0, 0, 1);
            old_angle = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
        }
        start_time = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        if (isClientOnly)
            return;

        if ((Time.time - start_time) < delay_time) return;

        angle += direction * speed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(old_angle + value * angle);

        if (angle > max_value)
        {
            if (axis == AXIS.X)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(max_value, old_angle.y, old_angle.z));
            }
            if (axis == AXIS.Y)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(old_angle.x, max_value, old_angle.z));
            }
            if (axis == AXIS.Z)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(old_angle.x, old_angle.y, max_value));
            }
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            direction = -1;
        }
        else if (angle < min_value)
        {
            if (axis == AXIS.X)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(min_value, old_angle.y, old_angle.z));
            }
            if (axis == AXIS.Y)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(old_angle.x, min_value, old_angle.z));
            }
            if (axis == AXIS.Z)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(old_angle.x, old_angle.y, min_value));
            }
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            direction = 1;
        }
    }
}
