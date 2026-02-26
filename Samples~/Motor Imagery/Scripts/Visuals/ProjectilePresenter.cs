using System;
using System.Collections;
using BCIEssentials.Extensions;
using UnityEngine;

public class ProjectilePresenter : MonoBehaviour
{
    public ObjectFrame[] Frames;

    private Coroutine _displayRoutine;


    private void Start() => HideFrames();

    public void HideFrames()
    {
        if (_displayRoutine != null)
        {
            StopCoroutine(_displayRoutine);
        }
        foreach (ObjectFrame frame in Frames) frame.Hide();
    }

    public void DisplayProjectile()
    {
        _displayRoutine = StartCoroutine(RunProjectileDisplay());
    }

    private IEnumerator RunProjectileDisplay()
    {
        foreach (ObjectFrame frame in Frames)
        {
            yield return frame.RunDisplay();
        }
        _displayRoutine = null;
    }


    [Serializable]
    public class ObjectFrame
    {
        public GameObject DisplayObject;
        public float DisplayPeriod = 0.1f;


        public void Hide() => DisplayObject.Deactivate();

        public IEnumerator RunDisplay()
        {
            DisplayObject.Activate();
            yield return new WaitForSeconds(DisplayPeriod);
            DisplayObject.Deactivate();
        }
    }
}