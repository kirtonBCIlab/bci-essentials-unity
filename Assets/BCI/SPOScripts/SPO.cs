using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Base class for the Stimulus Presenting Objects (SPOs)

public class SPO : MonoBehaviour
{
    public Color onColour;              //Color during the 'flash' of the object.
    public Color offColour;             //Color when not flashing of the object.

    // Whether or not to include in the Controller object, used to change which objects are selectable
    public bool includeMe = true;
    public int myIndex;

    //Use a boolean to indicate whether or not this SPO has a subset image
    [SerializeField]
    public bool hasImageChild = false;


    // Turn the stimulus on
    public virtual float TurnOn()
    {
        //This is just for an object renderer (e.g. 3D object). Use <SpriteRenderer> for 2D
        { this.GetComponent<Renderer>().material.color = onColour; }


        //Return time since stim
        return Time.time;


    }

    // Turn off/reset the SPO
    public virtual void TurnOff()
    {
        //This is just for an object renderer (e.g. 3D object). Use <SpriteRenderer> for 2D
        { this.GetComponent<Renderer>().material.color = offColour; }
    }

    // What to do on selection
    public virtual void OnSelection()
    {
        // This is free form, do whatever you want on selection

        StartCoroutine(QuickFlash());

        // Reset
        TurnOff();
    }

    // What to do when targeted for training selection
    public virtual void OnTrainTarget()
    {
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue, objectScale.z * scaleValue);
    }

    // What to do when untargeted
    public virtual void OffTrainTarget()
    {
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue, objectScale.z / scaleValue);
    }

    // Quick Flash
    public IEnumerator QuickFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            TurnOn();
            yield return new WaitForSecondsRealtime(0.2F);
            TurnOff();
            yield return new WaitForSecondsRealtime(0.2F);
        }
    }

}
