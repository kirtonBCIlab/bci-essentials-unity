using BCIEssentials.ControllerBehaviors;
using BCIEssentials.LSLFramework;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    /// <summary>
    /// Standalone version of <see cref="BCIController"/> that inherits
    /// from <see cref="MIControllerBehavior"/>.
    /// </summary>
    public class MIController : MIControllerBehavior
    {
        //Display
        public int refreshRate = 60;
        private float currentRefreshRate;
        private float sumRefreshRate;
        private float avgRefreshRate;
        private int refreshCounter = 0;

        protected override void Start()
        {
            // Attach Scripts
            Initialize(GetComponent<LSLMarkerStream>(), GetComponent<LSLResponseStream>());

            // Set the target framerate
            Application.targetFrameRate = refreshRate;
        }

        private void Update()
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
                    Debug.Log($"Refresh rate is below 95% of target, avg refresh rate {avgRefreshRate}");
                }

                sumRefreshRate = 0;
                refreshCounter = 0;
            }



            // Check key down

            // Press S to start/stop stimulus
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartStopStimulusRun();
            }

            // Press T to do automated training
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartTraining(BCITrainingType.Automated);
            }

            // Press I to do Iterative training (MI only)
            if (Input.GetKeyDown(KeyCode.I))
            {
                StartTraining(BCITrainingType.Iterative);
            }

            // Press U to do User training, stimulus without BCI
            if (Input.GetKeyDown(KeyCode.U))
            {
                StartTraining(BCITrainingType.User);
            }


            // Check for a selection if stim is on
            if (StimulusRunning)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    SelectSPOAtEndOfRun(0);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SelectSPOAtEndOfRun(1);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SelectSPOAtEndOfRun(2);
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SelectSPOAtEndOfRun(3);
                }

                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SelectSPOAtEndOfRun(4);
                }

                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    SelectSPOAtEndOfRun(5);
                }

                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    SelectSPOAtEndOfRun(6);
                }

                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    SelectSPOAtEndOfRun(7);
                }

                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    SelectSPOAtEndOfRun(8);
                }

                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    SelectSPOAtEndOfRun(9);
                }
            }
        }
    }
}