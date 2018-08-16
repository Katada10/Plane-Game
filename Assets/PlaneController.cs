using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour {

#region variables
    public float  maxSpeed, takeOffSpeed, lift, turn, drag ;
    public float xLimDown, xLimUp, zLimDown, zLimUp, minMoveSpeed;
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
    
    void CheckAllowTakeOff(float v)
    {
        if(thrust.value >= takeOffSpeed)
        {
            if (!inAir)
            {
                rb.velocity += -Vector3.up * v * lift * Time.deltaTime;
                rb.drag = 0;
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

                if(thrust.value < minMoveSpeed)
                {
                    rb.drag = 1;
                    rb.useGravity = true;
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

    void FixedUpdate () {

        
            #region thrust
        if(autoPilot)
        {
            thrust.interactable = false;
            thrust.value = auto.speed;
        }
            rb.AddRelativeForce(Vector3.forward * thrust.value, ForceMode.Force);
            if (rb.velocity.z == maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * thrust.value;
            }
            #endregion

            #region takeoff
            var v = Input.GetAxis("Vertical");
            CheckAllowTakeOff(v);

            #endregion

            #region turn
            var h = Input.GetAxisRaw("Horizontal");
            Roll(h);

            #endregion
        

    }

}
