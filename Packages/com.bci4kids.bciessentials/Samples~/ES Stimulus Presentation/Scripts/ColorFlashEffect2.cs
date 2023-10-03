using System.Collections;
using UnityEngine;
using BCIEssentials.Utilities;
using System.Collections.Generic;

namespace BCIEssentials.StimulusEffects
{
      /// <summary>
    /// Assign or Flash a renderers material color.
    /// </summary>
    public class ColorFlashEffect2 : StimulusEffect
    {
        [SerializeField]
        [Tooltip("The renderer to assign the material color to")]
        public Renderer _renderer;

        [SerializeField]
        public Material[] materialList;

        [Header("Flash Settings")]
        [SerializeField]
        [Tooltip("Material Color to assign while flashing is on")]
        private Color _flashOnColor = Color.red;
        
        [SerializeField]
        [Tooltip("Material Color to assign while flashing is off")]
        private Color _flashOffColor = Color.white;

        [SerializeField]
        [Tooltip("If the flash on color is applied on start or the flash off color.")]
        private bool _startOn;
        
        private float _flashDurationSeconds = 0.2f;

        private int _flashAmount = 3;

        public bool IsPlaying => _effectRoutine != null;

        private Coroutine _effectRoutine;

        public bool _chooseColorAndContrast;

        public enum FlashOnColor
        {
            Grey
        }
        public FlashOnColor _selectedColor;

        public enum ContrastLevel
        {
            Max,
            OneStepDown,
            TwoStepsDown,
            Min,
            Off
        }
        public ContrastLevel _contrastLevel;

        //Texture options
        public bool _chooseTexture;
        public enum TextureSelection 
        {
            Worms,
            Static,
            Wood,
            Voronoi, 
            Checkerboard,
            SqaureGrating
        }

        public TextureSelection _textureChoice;

        private float _textureOn1Float;
        private float _textureOff1Float;
        private float _textureOn2Float_1;
        private float _textureOn2Float_2;
        private float _textureOff2Float_1;
        private float _textureOff2Float_2;
        private Color _textureOnColor1;
        private Color _textureOnColor2;
        private Color _textureOffColor1;
        private Color _textureOffColor2;

        public Material[] setMaterials;

        private void Start()
        {
            if (_renderer == null && !gameObject.TryGetComponent(out _renderer))
            {
                Debug.LogWarning($"No Renderer component found for {gameObject.name}");
                return;
            }

            if (_renderer.material == null)
            {
                Debug.LogWarning($"No material assigned to renderer component on {gameObject.name}.");
            }

            if(_chooseColorAndContrast)
            {
                ContrastController();
                _flashOffColor = Color.black;
                AssignMaterialColor(_startOn ? _flashOnColor: _flashOffColor);
            }
         
            if(_chooseTexture)
            {
                TextureController();

                //backdrop material for the transparent textures
                setMaterials[1] = materialList[0];
                if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
                {
                    if(_textureChoice == TextureSelection.Worms)
                    {
                        setMaterials[0] = materialList[1];
                        _renderer.materials = setMaterials;
                    } 

                    if(_textureChoice == TextureSelection.Wood)
                    {
                        setMaterials[0] = materialList[2];
                        _renderer.materials = setMaterials;
                    }    
                    
                    if(_textureChoice == TextureSelection.Voronoi)
                    {
                        setMaterials[0] = materialList[3];
                        _renderer.materials = setMaterials;
                    }

                    SetTexture2(_textureOn2Float_1, _textureOn2Float_2);
                }

                else if(_textureChoice == TextureSelection.Static || _textureChoice ==  TextureSelection.SqaureGrating)
                {
                    if(_textureChoice == TextureSelection.Static)
                        _renderer.material = materialList[4];
                    
                    if(_textureChoice == TextureSelection.SqaureGrating)
                        _renderer.material = materialList[5];

                    SetTexture1(_textureOn1Float);
                }

                else 
                {
                    _renderer.material = materialList[6];
                    SetTextureColors2(_textureOnColor1, _textureOnColor2);
                }
            }
            else
                AssignMaterialColor(_startOn ? _flashOnColor: _flashOffColor);
        }

        public override void SetOn()
        {
            if (_renderer == null || _renderer.material == null)
                return;

            if(_chooseTexture)
            {
                if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
                    SetTexture2(_textureOn2Float_1, _textureOn2Float_2);
                else if(_textureChoice == TextureSelection.Static || _textureChoice == TextureSelection.SqaureGrating)
                    SetTexture1(_textureOn1Float);
                else 
                    SetTextureColors2(_textureOnColor1, _textureOnColor2);   
            }

            else
                AssignMaterialColor(_flashOnColor);

            IsOn = true;
        }

        public override void SetOff()
        {
            if (_renderer == null || _renderer.material == null)
            {
                return;
            }
            
            if(_chooseTexture)
            {
                if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
                    SetTexture2(_textureOff2Float_1, _textureOff2Float_2);
                else if(_textureChoice == TextureSelection.Static || _textureChoice == TextureSelection.SqaureGrating)
                    SetTexture1(_textureOff1Float);
                else 
                    SetTextureColors2(_textureOffColor1, _textureOffColor2);  
            }

            else
            {
                AssignMaterialColor(_flashOffColor);
            }
            IsOn = false;
        }

        public void Play()
        {
            Stop();
            _effectRoutine = StartCoroutine(RunEffect());

        }

        public void Stop()
        {
            if (!IsPlaying)
            {
                return;
            }

            SetOff();
            StopCoroutine(_effectRoutine);
            _effectRoutine = null;
        }

        private IEnumerator RunEffect()
        {
            if (_renderer != null && _renderer.material != null)
            {
                IsOn = true;
                
                for (var i = 0; i < _flashAmount; i++)
                {
                    //Deliberately not using SetOn and SetOff here
                    //to avoid excessive null checking
                    if(_chooseTexture)
                    {
                        if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
                            SetTexture2(_textureOn2Float_1, _textureOn2Float_2);
                        else if(_textureChoice == TextureSelection.Static || _textureChoice == TextureSelection.SqaureGrating)
                            SetTexture1(_textureOn1Float);
                        else 
                            SetTextureColors2(_textureOnColor1, _textureOnColor2);   
                    }

                    else
                        AssignMaterialColor(_flashOnColor);

                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);

                    if(_chooseTexture)
                    {
                        if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
                            SetTexture2(_textureOff2Float_1, _textureOff2Float_2);
                        else if(_textureChoice == TextureSelection.Static || _textureChoice == TextureSelection.SqaureGrating)
                            SetTexture1(_textureOff1Float);
                        else 
                            SetTextureColors2(_textureOffColor1, _textureOffColor2);   
                    }
                    else
                        AssignMaterialColor(_flashOffColor);
                    
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);
                }
            }
            SetOff();
            _effectRoutine = null;
        }

        private void ContrastController()
        {
            _flashOffColor = Color.black;
            ColorContrast colorContrast = GetComponent<ColorContrast>();
            int contrastIntValue = ConvertContrastLevel(_contrastLevel);
            colorContrast.SetContrast(contrastIntValue);

            if (_selectedColor == FlashOnColor.Grey)
                _flashOnColor = colorContrast.Grey();
            else 
                Debug.LogWarning("Selected Color not Available.");
        }

        //note that the int values are arbitrary
        public int ConvertContrastLevel(ContrastLevel _contrastLevel)
        {
            if(_contrastLevel == ContrastLevel.Max)
                return 100;
            else if (_contrastLevel == ContrastLevel.OneStepDown)
                return 50;
            else if (_contrastLevel == ContrastLevel.TwoStepsDown)
                return 25;
            else if (_contrastLevel == ContrastLevel.Min)
                return 10;
            else return 0;
        }

        private void TextureController()
        {

            if(_textureChoice == TextureSelection.Worms)
            {
                _textureOn2Float_1 = 0.5f;
                _textureOn2Float_1 = 0.2f;
                _textureOff2Float_1 = 0.5f;
                _textureOff2Float_2 = 1f;
            }
            else if(_textureChoice == TextureSelection.Wood)
            {
                _textureOn2Float_1 = 1f;
                _textureOn2Float_2 = 1f;
                _textureOff2Float_1 = 0f;
                _textureOff2Float_2 = 0.85f;
            }
            else if(_textureChoice == TextureSelection.Static)
            {
                _textureOn1Float = (1000f);
                _textureOff1Float = (0f);
            }
            else if(_textureChoice == TextureSelection.Voronoi)
            {
                _textureOn2Float_1 = 1f;
                _textureOn2Float_2 = 0.5f;
                _textureOff2Float_1 = 0.1f;
                _textureOff2Float_2 = 0.8f;
            }
            else if(_textureChoice == TextureSelection.Checkerboard)
            {
                _textureOnColor1 = Color.black;
                _textureOnColor2 = Color.white;
                _textureOffColor1 = Color.white;
                _textureOffColor2 = Color.black;
            }
            else if(_textureChoice == TextureSelection.SqaureGrating)
            {
                _textureOn1Float = (180f);
                _textureOff1Float = (0f);
            }
            else 
            {
                Debug.LogWarning("NOT WORKING");
            }
        }

        private void AssignMaterialColor(Color color)
        {
            _renderer.material.color = color;
        }

        public void SetTexture2(float val, float val2)
        {
            _renderer.material.SetFloat("_Float", val);
            _renderer.material.SetFloat("_Float2", val2);
        }
        
        public void SetTexture1(float val)
        {
            _renderer.material.SetFloat("_Float", val);
        }

        public void SetTextureColors2(Color c1, Color c2)
        {
            _renderer.material.SetColor("_Color1", c1);
            _renderer.material.SetColor("_Color2", c2);
        }

        public void SetContrast(ContrastLevel x, FlashOnColor y)
        {
            _selectedColor = y;
            _contrastLevel = x;
            ContrastController();
            if(setMaterials[0] != null)
            {
                _chooseTexture = false;
                setMaterials[0] = materialList[7]; 
                setMaterials[1] = null;
                _renderer.materials = setMaterials;
                _flashOffColor = Color.black;
            }
            else
                _chooseTexture = false;
        }


        public void SetColor(FlashOnColor x)
        {
            _chooseTexture = false;
            _selectedColor = x;
            ContrastController();
            if(setMaterials[0] != null )
            {
                if(_textureChoice == TextureSelection.Static)
                    setMaterials[0] = materialList[4]; 
                else
                    setMaterials[0] = materialList[7];

                setMaterials[1] = null;
                _renderer.materials = setMaterials;
                _flashOffColor = Color.black;
            }
        }

        public void SetTextureExternal(TextureSelection x)
        {
            _chooseTexture = true;
            _textureChoice = x;
            TextureController();
            
            if(_textureChoice == TextureSelection.Worms || _textureChoice == TextureSelection.Wood || _textureChoice == TextureSelection.Voronoi)
            {
                setMaterials[1] = materialList[0];
                    if(_textureChoice == TextureSelection.Worms)
                    {
                        setMaterials[0] = materialList[1];
                        _renderer.materials = setMaterials;
                    } 

                    if(_textureChoice == TextureSelection.Wood)
                    {
                        setMaterials[0] = materialList[2];
                        _renderer.materials = setMaterials;
                    }    
                    
                    if(_textureChoice == TextureSelection.Voronoi)
                    {
                        setMaterials[0] = materialList[3];
                        _renderer.materials = setMaterials;
                    }

                    SetTexture2(_textureOn2Float_1, _textureOn2Float_2);
                }

                else if(_textureChoice == TextureSelection.Static || _textureChoice ==  TextureSelection.SqaureGrating)
                {
                    if(_textureChoice == TextureSelection.Static)
                    {
                        setMaterials[1] = materialList[4];
                        setMaterials[0] = materialList[4];
                        _renderer.materials = setMaterials;
                    }

                    
                    if(_textureChoice == TextureSelection.SqaureGrating)
                    {
                        setMaterials[0] = materialList[5];
                        setMaterials[1] = materialList[8];
                        _renderer.materials = setMaterials;
                    }

                    SetTexture1(_textureOn1Float);
                }

                else 
                {
                    setMaterials[0] = materialList[6];
                    setMaterials[1] = materialList[8];
                    _renderer.materials = setMaterials;

                    SetTextureColors2(_textureOnColor1, _textureOnColor2);
                }

            }
        }
    }