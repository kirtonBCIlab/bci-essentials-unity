using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_SPO : MonoBehaviour
{
    //Variables

    public string spoText;
    public Image spoImage;

    //Private Variables
    private bool hasTextProp = false;
    private bool hasImageProp = false;


    //Awake
    public void Awake()
    {
        //Initialize as needed the spoText or the spoImage

        //Run the CheckProperty function
        CheckProperty();

        if (hasTextProp)
        {
            spoText = this.GetComponentInChildren<Text>().text;
        }
        else //Initialize as empty string.
        {
            spoText = "";
        }

        if(hasImageProp)
        {
            spoImage = GetComponentInChildren<Image>();
        }

        Debug.Log("Hey! Here is yoru text you fool! : " + spoText);
        Debug.Log("Hey! You also have an image. Cool cool cool. " + hasImageProp);
    }


    private void Update()
    {


    }

    //Set spotext string
    public void SetText(string myNewtext)
    {
        spoText = myNewtext;
        GetComponentInChildren<Text>().text = spoText;
    }

    //Behavior for Turning On
    public void TurnOn()
    {
        if (hasTextProp)
        {
            GetComponent<Image>().color = Color.red;
        }

        if(hasImageProp)
        {
            GetComponentInChildren<Image>().color = Color.red;
        }
    }

    //Behavior for Turning Off
    public void TurnOff()
    {
        if (hasTextProp)
        {
            GetComponent<Image>().color = Color.grey;
        }

        if(hasImageProp)
        {
            GetComponentInChildren<Image>().color = Color.grey;
        }
    }

    public virtual void OnSelection()
    {
        print("Selecting one of " + spoText);
        print("!!!!!! HEY !!!! Here is the number of characters I have: " + spoText.Length);
        //Flash flash flash flash flash
        StartCoroutine(flashMe());

        //Send information up to game controller
        // GameEvents.current.BCISelection(spoText);
        SendMessageUpwards("SPOSelection", spoText);
    }

    private void CheckProperty()
    {
        if (GetComponentInChildren<Text>() != null)
        {
            hasTextProp = true;
        }

        if (GetComponentInChildren<Image>() !=null)
        {
            hasImageProp = true;
        }

    }

    // private int GetNumChars()
    // {
    //     int numChars = GetComponentInChildren<Text>().text.Length;
    //     return numChars;
    // }

    private IEnumerator flashMe()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSecondsRealtime(0.3F);
            TurnOn();
            yield return new WaitForSecondsRealtime(0.3F);
            TurnOff();
        }
    }

}
