using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MainSPO : SPO
{
    public Color newOnColor;
    public Color newOffColor;

    [SerializeField]
    private int myObjectId;
    private string myMessage;

    [SerializeField]
    private SwitchfromMainCamera camScript;

    [SerializeField]
    private BarController barController;
    // Start is called before the first frame update
    void Start()
    {
        if(newOnColor==null)
        {
            Debug.Log("Yooooo you forgot to edit the object in the editor!");
        }
        Debug.Log(GetMyId());
    }

    public override float TurnOn()
    {
        { this.GetComponent<Image>().color = newOnColor; }
        return Time.time;
    }

    public override void TurnOff()
    {
        { this.GetComponent<Image>().color = newOffColor; } 
        
    }


    public override void OnSelection()
    {
        switch (myObjectId) 
        {
            case 0:
                //Todo - change to display or camera
                camScript.RotationView();
                break;
            case 1:
                //Todo
                camScript.InclineView();
                break;
            case 2:
                //Todo
                camScript.ElevationView();
                break;
            case 3:
                //Todo
                barController.DropButtonPressed();
                break;
        }
    }

    public int GetMyId()
    {
        return myObjectId;
    }

}
