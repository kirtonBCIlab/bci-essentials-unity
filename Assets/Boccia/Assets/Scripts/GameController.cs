using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Vector3 myPos;
    public Quaternion startPos;
    public GameObject mainShaft;

    public float rotZ;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        //record starting position
        startPos=mainShaft.transform.rotation;

        //mainShaft = GameObject.Find("MainShaft");
        rotZ = mainShaft.transform.localEulerAngles.y;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
