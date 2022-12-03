using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance = null;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }

}
