using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for the Stimulus Presenting Objects (SPOs)

public class SPO : MonoBehaviour
{
    public Color onColour;              //Color during the 'flash' of the object.
    public Color offColour;             //Color when not flashing of the object.

    // Whether or not to include in the Controller object, used to change which objects are selectable
    public bool includeMe = true;
    public int myIndex;

    // Target object
    //GameObject trainingCube;

    //private GameObject trainTarget;
    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    // Turn the stimulus on
    public virtual void turnOn()
    {
        this.GetComponent<Renderer>().material.color = onColour;
    }

    // Turn off/reset the SPO
    public virtual void turnOff()
    {
        this.GetComponent<Renderer>().material.color = offColour;
    }

    // What to do on selection
    public virtual void onSelection()
    {
        // This is free form, do whatever you want on selection

        StartCoroutine(quickFlash());
 
        // Reset
        turnOff();
    }

    // What to do when targeted for training selection
    public virtual void onTrainTarget()
    {
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue, objectScale.z * scaleValue);
    }

    // What to do when untargeted
    public virtual void offTrainTarget()
    {
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue, objectScale.z / scaleValue);
    }

    // Quick Flash
    private IEnumerator quickFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            turnOn();
            yield return new WaitForSecondsRealtime(0.2F);
            turnOff();
            yield return new WaitForSecondsRealtime(0.2F);
        }
    }

}
