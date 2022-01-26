using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class GameEvents : MonoBehaviour
{

    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action onBCISelection;
    public void BCISelection(string mySelection)
    {
        if(onBCISelection !=null)
        {
            onBCISelection();
        }
    }


    public event Action onStepOneSelection;
    public void StepOneSelectionAction()
    {
        if(onStepOneSelection != null)
        {
            onStepOneSelection();
        }
    }

    public event Action onStepTwoSelection;
    public void StepTwoSelectionAction()
    {
        if(onStepTwoSelection != null)
        {
            onStepTwoSelection();
        }
    }

}
