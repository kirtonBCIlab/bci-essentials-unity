using UnityEngine;

[System.Serializable]
public class ColourMaskFlashBehaviour
{
    public Color OnColour = Color.white;
    public Color OffColour = Color.black;

    private Material TargetMaterial => _renderer.material;
    private Renderer _renderer;

    public void InitializeRenderer(Renderer target)
    {
        _renderer = target;
        if (!_renderer)
        {
            Debug.LogWarning("Missing renderer component reference");
            return;
        }

        Shader colourMaskShader = Shader.Find("Unlit/Colour Mask");
        _renderer.material = new(colourMaskShader);
        SetMaskEnabled(false);
    }


    public void DisplayOnColour() => SetMaskColour(OnColour);
    public void DisplayOffColour() => SetMaskColour(OffColour);

    public void SetMaskEnabled(bool value)
    => TargetMaterial.SetFloat("_MaskEnabled", value ? 1 : 0);

    public void SetMaskColour(Color value)
    => TargetMaterial.SetColor("_MaskColour", value);
}