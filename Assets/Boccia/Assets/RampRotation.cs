using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RampRotation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mainShaft; 
    float targetAngle = 90.0f;
    float currentAngle; 
    public void RotateLeftS() {
        changeAngle(-2.0f);
    }
    public void RotateLeftM() {
        changeAngle(-4.0f);
        //currentAngle= mainShaft.transform.Rotation.z; 
    }
    public void RotateRightS() {
        changeAngle(2.0f);
        //currentAngle= mainShaft.transform.rotation.z;  
    }
    public void RotateRightM() {
        changeAngle(4.0f);
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
        currentAngle = mainShaft.transform.localEulerAngles.y; 
        Debug.Log(targetAngle + ":" + currentAngle);
        float x = 10.0f;
        if ((currentAngle-targetAngle) < 0.15 & (currentAngle-targetAngle) > -0.15) {
            x=0;
        }
        else if (targetAngle < currentAngle) {
            mainShaft.transform.Rotate(Vector3.forward, -x * Time.deltaTime);
        }
        else if (targetAngle>currentAngle) {
            mainShaft.transform.Rotate(Vector3.forward, x*Time.deltaTime);
        }
    }
}
