using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour
{
    public float maxSpeed, takeOffSpeed, lift, turn, drag, weakDrag;
    public float xLimDown, xLimUp, zLimDown, zLimUp, minMoveSpeed;
    public float SliderSensitivity;
    public Slider thrust;
    
    private Rigidbody rb;
    private bool inAir, autoPilot = true;
    private AutoPilot auto;

    void Start()
    {
        inAir = false;
        thrust.maxValue = maxSpeed;

        auto = GetComponent<AutoPilot>();
        rb = GetComponent<Rigidbody>();
    }

    float SliderFill(float currentValue, float target)
    {
        if (auto.speed != 0)
        {
            currentValue = Mathf.MoveTowards(currentValue, target, Time.deltaTime * (SliderSensitivity += 0.05f));
            return currentValue;
        }
        return 0f;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name == "Terrain")
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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z), Time.deltaTime * 1);
        }
    }

    void RecalibratePitch()
    { 
        if (transform.rotation.eulerAngles.x != 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z)), Time.deltaTime * 1);
        }
        rb.constraints = RigidbodyConstraints.None;
    }

    void RecalibratePitchH(float h)
    {
        if (h == 0 && transform.rotation.eulerAngles.z != 0)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 1);
        }
    }

    void CheckAllowTakeOff(float v)
    {
        if (thrust.value >= takeOffSpeed && thrust.value >= minMoveSpeed)
        {
            if (!inAir)
            {
                rb.velocity += -Vector3.up * v * lift * Time.deltaTime;
                rb.drag = weakDrag;
            }
            else
            {
                rb.useGravity = false;
                rb.drag = drag;


                if (Mathf.Floor(transform.eulerAngles.x) < xLimDown || Mathf.Floor(transform.eulerAngles.x) > xLimUp)
                {
                    transform.Rotate(v * lift * Time.deltaTime, 0, 0);
                }
              
                RecalibratePitch(v);
            }
        }
        
    }

    void PilotTakeOff()
    {
        if (thrust.value >= takeOffSpeed)
        {
            if (!inAir)
            {
                rb.velocity += Vector3.up * lift * Time.deltaTime;
                rb.drag = weakDrag;
            }
            else
            {
                rb.useGravity = false;
                rb.drag = drag;

                if (Mathf.Floor(transform.eulerAngles.x) < xLimDown || Mathf.Floor(transform.eulerAngles.x) > xLimUp)
                {
                    transform.Rotate(-lift * Time.deltaTime, 0, 0);
                }

                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-15, transform.eulerAngles.y, transform.eulerAngles.z), Time.deltaTime * 1);

                if (inAir && Mathf.Ceil(transform.position.y) == (auto.height))
                {
                    rb.constraints = RigidbodyConstraints.FreezePositionY;
                    RecalibratePitch();
                }
            }
        }
    }

    void Roll(float h)
    {

        if (Mathf.Floor(transform.eulerAngles.z) <= zLimDown || Mathf.Floor(transform.eulerAngles.z) >= zLimUp)
        {
            transform.Rotate(0, 0, turn * -h * Time.deltaTime, Space.Self);

        }
        if (h != 0)
            transform.Rotate(-lift * Time.deltaTime, 0, 0);
        
        RecalibratePitchH(h);
    }

    void PilotRoll()
    {
        Quaternion target = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, auto.heading));
        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 1);

        if (auto.heading != 0)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            transform.Rotate(-lift * Time.deltaTime, 0, 0);
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    void TakeOff()
    {
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
            rb.drag = weakDrag;
        }
    }

    void Rotating()
    {
        if (!autoPilot)
        {
            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxisRaw("Horizontal");
            
            CheckAllowTakeOff(v);
            Roll(h);
        }
        else
        {
            PilotTakeOff();
            PilotRoll();
        }
    }

    void FixedUpdate()
    {
        TakeOff();
        Rotating();
    }
}
