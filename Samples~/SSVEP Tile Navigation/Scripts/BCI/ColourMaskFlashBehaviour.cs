using UnityEngine;
using BCIEssentials.Stimulus.Presentation.Standard;
using System.Collections;

public class ColourMaskFlashBehaviour : ColourFlashBehaviour
{
    [SerializeField, Space]
    private Shader _colourMaskShader;
    private Material _material;


    protected override void Awake()
    {
        Color? inspectorTint = _renderer != null ? _renderer.material.color : null;
        base.Awake();

        _material = _renderer.material = new(_colourMaskShader);
        SetMaskEnabled(false);
        if (inspectorTint.HasValue)
        {
            _material.SetColor("_Color", inspectorTint.Value);
        }
    }

    private void OnDisable() => SetMaskEnabled(false);


    public override void SetColour(Color colour)
    {
        if (enabled || IsFlashing) SetMaskEnabled(true);
        SetMaskColour(colour);
    }

    public void SetMaskEnabled(bool value)
    {
        if (_material)
        {
            _material.SetFloat("_MaskEnabled", value ? 1 : 0);
        }
    }

    public void SetMaskColour(Color value)
    {
        if (_material)
        {
            _material.SetColor("_MaskColour", value);
        }
    }


    protected override IEnumerator RunFlashes(float period, int count)
    {
        enabled = true;
        SetMaskEnabled(true);
        yield return base.RunFlashes(period, count);
        SetMaskEnabled(false);
    }
}