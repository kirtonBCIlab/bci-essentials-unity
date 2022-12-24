using UnityEngine;
using System;
using BCIEssentials.ControllerBehaviors;

public class P300Controller : P300ControllerBehavior
{
    //Display
    public int refreshRate = 60;
    private float currentRefreshRate;
    private float sumRefreshRate;
    private float avgRefreshRate;
    private int refreshCounter = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Attach Scripts
        setup = GetComponent<Matrix_Setup>();
        Initialize(GetComponent<LSLMarkerStream>(), GetComponent<LSLResponseStream>());
        
        // Set the target framerate
        Application.targetFrameRate = refreshRate;
    }

    // Update is called once per frame
    void Update()
    {
        // Check the average framerate every second
        currentRefreshRate = 1 / Time.deltaTime;
        refreshCounter += 1;
        sumRefreshRate += currentRefreshRate;
        if (refreshCounter >= refreshRate)
        {
            avgRefreshRate = sumRefreshRate / (float)refreshCounter;
            if (avgRefreshRate < 0.95 * (float)refreshRate)
            {
                Debug.Log("Refresh rate is below 95% of target, avg refresh rate " + avgRefreshRate.ToString());
            }

            sumRefreshRate = 0;
            refreshCounter = 0;
        }



        // Check key down

        // Press S to start/stop stimulus
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartStopStimulus();
        }

        // Press T to do automated training
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoTraining());
        }

        // Press I to do Iterative training (MI only)
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoIterativeTraining());
        }

        // Press U to do User training, stimulus without BCI
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine(DoUserTraining());
        }


        // Check for a selection if stim is on
        if (stimOn )
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StartCoroutine(SelectObjectAfterRun(0));
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(SelectObjectAfterRun(1));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(SelectObjectAfterRun(2));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartCoroutine(SelectObjectAfterRun(3));
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartCoroutine(SelectObjectAfterRun(4));
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(SelectObjectAfterRun(5));
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                StartCoroutine(SelectObjectAfterRun(6));
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                StartCoroutine(SelectObjectAfterRun(7));
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                StartCoroutine(SelectObjectAfterRun(8));
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                StartCoroutine(SelectObjectAfterRun(9));
            }
        }
    }
}
