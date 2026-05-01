using UnityEngine;
using BCIEssentials.Stimulus;
using System.Collections;

public class ColourMaskFlashBehaviour : ColourFlashBehaviour
{
    [SerializeField, Space]
    private Shader _colourMaskShader;
    private Material _material;


    public override void SetUp()
    {
        Color? inspectorTint = _renderer != null ? _renderer.material.color : null;
        base.SetUp();

        _material = _renderer.material = new(_colourMaskShader);
        SetMaskEnabled(false);
        if (inspectorTint.HasValue)
        {
            _material.SetColor("_Color", inspectorTint.Value);
        }
    }


    public override void SetColour(Color colour)
    {
        if (IsFlashing) SetMaskEnabled(true);
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
        SetMaskEnabled(true);
        yield return base.RunFlashes(period, count);
        SetMaskEnabled(false);
    }
}