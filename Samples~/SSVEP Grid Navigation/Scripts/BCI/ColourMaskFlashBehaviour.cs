using UnityEngine;
using BCIEssentials.Stimulus.Presentation.Standard;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class ColourMaskFlashBehaviour : ColourFlashBehaviour
{
    [SerializeField, Space]
    private Shader _colourOverrideShader;
    private Material _material;


    private void Start()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _material = renderer.material = new(_colourOverrideShader);
        _material.SetColor("_Color", renderer.color);
        SetColourOverrideEnabled(false);
    }

    private void OnDisable() => SetColourOverrideEnabled(false);


    public override void SetColour(Color colour)
    {
        if (enabled || IsFlashing) SetColourOverrideEnabled(true);
        SetOverrideColour(colour);
    }

    public void SetColourOverrideEnabled(bool value)
    {
        if (_material)
        {
            _material.SetFloat("_OverrideEnabled", value ? 1 : 0);
        }
    }

    public void SetOverrideColour(Color value)
    {
        if (_material)
        {
            _material.SetColor("_OverrideColour", value);
        }
    }


    protected override IEnumerator RunFlashes(float period, int count)
    {
        enabled = true;
        SetColourOverrideEnabled(true);
        yield return base.RunFlashes(period, count);
        SetColourOverrideEnabled(false);
    }
}