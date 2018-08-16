using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour {

#region variables
    public float  maxSpeed, takeOffSpeed, lift, turn, drag ;
    public float xLimDown, xLimUp, zLimDown, zLimUp, minMoveSpeed;
    public float SliderSmoothness;
    public Slider thrust;
    public Transform target;
    
    private Rigidbody rb;
    private bool inAir, autoPilot = true;
    private AutoPilot auto;

    #endregion

    void Start()
    {
        inAir = false;
        auto = GetComponent<AutoPilot>();
        thrust.maxValue = maxSpeed;
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.name == "Terrain")
        {
            inAir = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Terrain")
        {
            inAir = false;
        }
    }

    void RecalibratePitch(float v)
    {
        if (v == 0 && transform.rotation.eulerAngles.x != 0)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 1);
        }
    }

    void RecalibratePitch()
    {
        Quaternion targ = Quaternion.Euler(new Vector3(45, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, targ, Time.deltaTime * 1);
    }

    void CheckAllowTakeOff(float v)
    {
        if(thrust.value >= takeOffSpeed)
        {
            if (!inAir)
            {
                rb.velocity += -Vector3.up * v * lift * Time.deltaTime;
                rb.drag = 1;
            }
            else
            {
                rb.useGravity = false;
                rb.drag = drag;
                transform.Rotate(v * lift * Time.deltaTime, 0, 0);
                
                if (transform.eulerAngles.x >= xLimDown && transform.eulerAngles.x <= (xLimDown + xLimUp))
                {
                    if(v < 0)
                    {
                        transform.rotation = Quaternion.Euler(xLimDown + xLimUp, transform.eulerAngles.y, transform.eulerAngles.z);
                    }
                    if (v > 0)
                    {
                       transform.rotation = Quaternion.Euler(xLimDown, transform.eulerAngles.y, transform.eulerAngles.z);
                    }
                }
                
                RecalibratePitch(v);
            }
        }
        
    }
    
    void Roll(float h)
    {
        transform.Rotate(0, 0, turn * -h * Time.deltaTime, Space.Self);
        if (transform.rotation.z <= -0.1 || transform.rotation.z >= 0.1)
        {
            transform.Rotate(-lift * Time.deltaTime, 0, 0);
        }

        if (transform.eulerAngles.z >= zLimDown && transform.eulerAngles.z <= (zLimDown + zLimUp))
        {
            if (h > 0)
            {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zLimDown + zLimUp);
            }
            if (h < 0)
            {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zLimDown);
            }
        }

        RecalibratePitchH(h);
    }

    void RecalibratePitchH(float h)
    {
        if (h == 0 && transform.rotation.eulerAngles.z != 0)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 1);
        }
    }

    void PilotTakeOff()
    {
        if (thrust.value >= takeOffSpeed)
        {
            if (!inAir)
            {
                rb.velocity += Vector3.up  * lift * Time.deltaTime;
                rb.drag = 1;
            }
            else
            {
                rb.useGravity = false;
                rb.drag = drag;

                Quaternion target = Quaternion.Euler(new Vector3(-45, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
                transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 1);

                if (transform.position.y >= auto.height)
                {
                    RecalibratePitch();
                }
            }
        }
    }

    float SliderFill(float currentValue, float target)
    {
       currentValue = Mathf.MoveTowards(currentValue, target, Time.deltaTime * (SliderSmoothness += 0.1f));
        return currentValue;
    }

    void PilotRoll()
    {
        Quaternion target = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, auto.heading));
        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 1);

        if (auto.heading != 0)
        {
            transform.Rotate(-lift * Time.deltaTime, 0, 0);
        }

    }

    void FixedUpdate()
    {
        #region thrust
        if (autoPilot)
        {
            thrust.interactable = false;
            thrust.value = SliderFill(thrust.value, auto.speed);
        }
        rb.AddRelativeForce(Vector3.forward * thrust.value, ForceMode.Force);
        if (rb.velocity.z == maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * thrust.value;
        }
        if (rb.velocity.z < minMoveSpeed)
        {
            rb.useGravity = true;
            rb.drag = 1;
        }
        #endregion

        #region takeoff
        if (!autoPilot)
        {
            var v = Input.GetAxis("Vertical");
            CheckAllowTakeOff(v);
        }
        else
        {
           PilotTakeOff();
        }

        #endregion

        #region turn
        if (!autoPilot)
        {
            var h = Input.GetAxisRaw("Horizontal");
            Roll(h);
        }
        else
        {
            PilotRoll();
        }
        #endregion

    }

}
