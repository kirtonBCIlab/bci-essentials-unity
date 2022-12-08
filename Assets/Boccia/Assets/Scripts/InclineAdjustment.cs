using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclineAdjustment : MonoBehaviour
{
    public GameObject inclineAxis; 
    public GameObject hinge;
    public GameObject inclineActuator;
    public GameObject leadscrewNut;
    float targetAngle = 0.0f;
    float currentAngle= 0.0f;
    public void IncreaseAngle() {
        changeAngle(-2.0f);
    }
    public void DecreaseAngle() {
        changeAngle(2.0f);
        //currentAngle= mainShaft.transform.rotation.z;  
    }

    void changeAngle(float change){
        targetAngle += change;
        if (targetAngle>180f){
            targetAngle = 180f;
        } 
        else if (targetAngle < 0.0f){
            targetAngle = 0.0f;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(targetAngle + ":" + currentAngle);
        if (targetAngle < currentAngle) {
            //inclineAxis.transform.Rotate(-1.0f*Time.deltaTime, 0.0f, 0.0f, Space.Self);
            //inclineActuator.transform.
            //leadscrewNut.transform.
            //hinge.transform.Rotate(-0.5f*Time.deltaTime, 0.0f, 0.0f, Space.Self);
            currentAngle += -1.0f;
        }
        else if (targetAngle>currentAngle) {
            //inclineAxis.transform.Rotate(1.0f*Time.deltaTime, 0.0f, 0.0f, Space.Self);
            currentAngle += 1.0f;
        }
    }
}
