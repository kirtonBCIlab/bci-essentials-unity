using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P300_Events : MonoBehaviour
{
    //Event class to handle what happens when targets are selected!

    public static P300_Events current;

    private void Awake()
    {
        current = this;
    }

    //Public event
    public event Action OnTargetFlash;
    //Corresponding method
    public void TargetFlashEvent()
    {
        if (OnTargetFlash !=null)
        {
            OnTargetFlash();
        }
    }

    public event Action OnNonTargetFlash;

    public void NonTargetFlashEvent()
    {
        if(OnNonTargetFlash !=null)
        {
            OnNonTargetFlash();
        }
    }

    public event Action<int> OnTargetSelection;

    public void TargetSelectionEvent(int id)
    {
        if (OnTargetSelection != null)
        {
            OnTargetSelection(id);
        }
    }

    public event Action<int> OnBAPSelection;

    public void BAPSelectionEvent(int id)
    {
        if(OnBAPSelection != null)
        {
            OnBAPSelection(id);
        }
    }

}
