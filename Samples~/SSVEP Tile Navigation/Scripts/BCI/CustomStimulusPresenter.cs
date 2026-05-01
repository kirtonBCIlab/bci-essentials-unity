using BCIEssentials.Stimulus;
using UnityEngine.Events;

public class CustomStimulusPresenter : FrameCycleFrequencyStimulusPresenter
{
    public UnityEvent OnSelected;
    private ColourMaskFlashBehaviour CustomFlashBehaviour
    => (_colourFlashBehaviour is ColourMaskFlashBehaviour m) ? m : null;

    public override void Select()
    {
        OnSelected?.Invoke();
        base.Select();
    }

    protected override void SetUpStimulusDisplay()
    => CustomFlashBehaviour?.SetMaskEnabled(true);
    protected override void CleanUpStimulusDisplay()
    => CustomFlashBehaviour?.SetMaskEnabled(false);
}