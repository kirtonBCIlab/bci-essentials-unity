using BCIEssentials.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartStopButtonActivationLinker : MonoBehaviour
{
    public ButtonPair[] ButtonPairs;

    private void Start()
    {
        foreach(ButtonPair pair in ButtonPairs)
        {
            pair.ConnectEvents(ButtonPairs.Excluding(pair));
        }
    }


    [System.Serializable]
    public class ButtonPair
    {
        public Button StartButton;
        public Button StopButton;

        public void ConnectEvents(params ButtonPair[] linkedButtons)
        {
            StartButton.AddClickListeners(
                StopButton.ActivateGameObject,
                StartButton.DeactivateGameObject
            );
            StopButton.AddClickListeners
            (
                StopButton.DeactivateGameObject,
                StartButton.ActivateGameObject
            );
            foreach (ButtonPair pair in linkedButtons)
            {
                StartButton.AddClickListener(pair.Deactivate);
                StopButton.AddClickListener(pair.ActivateStartButton);
            }
        }

        public void Deactivate()
        {
            StartButton.DeactivateGameObject();
            StopButton.DeactivateGameObject();
        }

        public void ActivateStartButton()
        {
            StartButton.ActivateGameObject();
        }
    }
}

public static class ButtonExtensions
{
    public static void AddClickListeners(this Button caller, params UnityAction[] callbacks)
    {
        foreach (UnityAction method in callbacks) caller.onClick.AddListener(method);
    }

    public static void AddClickListener(this Button caller, UnityAction callback)
    {
        caller.onClick.AddListener(callback);
    }
}