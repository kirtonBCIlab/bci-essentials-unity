using BCIEssentials.Stimulus.Presentation.Standard;
using UnityEngine.Events;

public class CustomStimulusPresenter : FrameCycleFrequencyStimulusPresenter
{
    public UnityEvent OnSelected;

    public override void Select()
    {
        OnSelected?.Invoke();
        base.Select();
    }

    protected override void SetUpStimulusDisplay()
    {
        _colourFlashBehaviour.enabled = true;
    }
    protected override void CleanUpStimulusDisplay()
    {
        _colourFlashBehaviour.enabled = false;
    }
}