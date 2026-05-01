using BCIEssentials.Stimulus;
using UnityEngine;
using UnityEngine.Events;

public class CustomStimulusPresenter : FrequencyStimulusPresenter
{
    [SerializeField]
    private ColourMaskFlashBehaviour _customFrequencyDisplay;

    [Space]
    public UnityEvent OnSelected;


    protected override void Awake()
    {
        _customFrequencyDisplay.InitializeRenderer(_colourFlashBehaviour.Renderer);
        base.Awake();
    }


    public override void Select()
    {
        OnSelected?.Invoke();
        base.Select();
    }

    protected override void ToggleDisplayState(bool value)
    {
        if (value) _customFrequencyDisplay.DisplayOnColour();
        else _customFrequencyDisplay.DisplayOffColour();
    }

    protected override void SetUpStimulusDisplay()
    => _customFrequencyDisplay.SetMaskEnabled(true);
    protected override void CleanUpStimulusDisplay()
    => _customFrequencyDisplay.SetMaskEnabled(false);
}