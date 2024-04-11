using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffectKeyPress : MonoBehaviour
{
    Material material;
    bool isDissolving = false;
    private float fade = 1f;

    [Range(0f, 100f)]
    public float scale;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_Scale", scale);
        material.SetColor("_Color", color);
    }    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            isDissolving = true;
        }

        if (isDissolving)
        {
            fade -= Time.deltaTime/2;

            if (fade <= 0f)
            {
                fade = 0f;
                isDissolving = false;
            }
        
        material.SetFloat("_Fade", fade);
        }
    }
}

