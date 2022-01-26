using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //Public variables
    public P300_Controller p300_controller;
    public GameObject stepOnePanel;
    public GameObject stepTwoPanel;

    private void Awake()
    {
        //Initialize the game objects
            GameObject stepOnePanel = GameObject.Find("StepOne");
            GameObject stepTwoPanel = GameObject.Find("SepTwo4x");


    }

    private string spoTarg = "";
    private string[] spoTargs;

    // Start is called before the first frame update
    void Start()
    {
        //Set panels to off
        stepOnePanel.SetActive(false);
        stepTwoPanel.SetActive(false);
        //Subscribe to Events
        GameEvents.current.onStepOneSelection += OnStepOneSelection;
        GameEvents.current.onStepTwoSelection += OnStepTwoSelection;
        GameEvents.current.onBCISelection += OnBCISelection;
    }

    // Update is called once per frame
    void Update()
    {
        //Set up unit test on key G.
        if(Input.GetKeyDown(KeyCode.G))
        {
           stepOnePanel.SetActive(!stepOnePanel.activeSelf);
        }
    }

    //Get Message from SPO On Selection
    public void SPOSelection(string spoText)
    {
        //Populate the global variable with spoText
        spoTarg = spoText;

        //Parse string information
        spoTargs = spoTarg.Split(' ');

        //if this Selection1 do the following
        if(spoTargs.Length >=2)
        {
            //Turn off panel1
            stepOnePanel.SetActive(false);
            //Turn on next panel
            stepTwoPanel.SetActive(true);
            //Populate panel items with for loop
            for(int i = 0; i < spoTargs.Length; i++)
            {
                // Give the panel the correct text
                string correctText = spoTargs[i];
                
                
            }

            //Remove items that are not needed
            
        }

        //if this is Selection 2
        if(spoTargs.Length == 1)
        {

            print("You have selected " + spoTargs[0]);

            //Get string value, set it in the bottom written are


            //return to selection 1 panel
            stepOnePanel.SetActive(true);
            stepTwoPanel.SetActive(false);
        }
           
        if(spoTargs.Length == 0)
        {
            Debug.Log("oh deary me, the selection was empty");
        }
        
    }

    //Deal with what happens during step one selection
    private void OnStepOneSelection()
    {

    }

    //Deal with what happens during step two selection
    private void OnStepTwoSelection()
    {

    }

    private void OnBCISelection()
    {

    }

}
